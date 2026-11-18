using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;

namespace MailNotification;

/// <summary>Lògica central: comptes, comprovació periòdica, safata del sistema, comptador i inici automàtic.</summary>
public sealed class Controller
{
    public static Controller Instance { get; private set; } = null!;

    public AppSettings Settings { get; private set; }
    public ObservableCollection<AccountVm> Accounts { get; } = new();

    MainWindow? _win;
    WinForms.NotifyIcon? _tray;
    System.Drawing.Icon? _trayIcon;
    readonly DispatcherTimer _timer = new();
    bool _checking;
    bool _quitting;
    AccountVm? _lastToast;

    public Controller()
    {
        Instance = this;
        Settings = SettingsStore.Load();
        Loc.I.Language = Settings.Language;
        foreach (var a in Settings.Accounts) Accounts.Add(new AccountVm(a));
    }

    public void Start(bool hidden)
    {
        _win = new MainWindow();
        _tray = new WinForms.NotifyIcon { Visible = true };
        _tray.MouseClick += (_, e) => { if (e.Button == WinForms.MouseButtons.Left) ShowWindow(); };
        _tray.BalloonTipClicked += (_, _) => { if (_lastToast != null) OpenGmail(_lastToast.Model); };
        Loc.I.PropertyChanged += (_, _) => { BuildTrayMenu(); UpdateBadge(); };
        BuildTrayMenu();
        UpdateBadge();

        _timer.Tick += async (_, _) => await CheckAllAsync();
        ApplySettings();

        if (!hidden) ShowWindow();
        _ = CheckAllAsync();
    }

    // ---------- Finestra ----------
    public void ShowWindow()
    {
        if (_win == null) return;
        _win.Show();
        if (_win.WindowState == System.Windows.WindowState.Minimized) _win.WindowState = System.Windows.WindowState.Normal;
        _win.Activate();
    }

    public bool CanHideToTray => !_quitting && Settings.MinimizeToTray;

    public void Quit()
    {
        _quitting = true;
        if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
        System.Windows.Application.Current.Shutdown();
    }

    public void OpenWindowOverlay(System.Windows.Media.ImageSource? img, string desc)
    {
        if (_win?.TaskbarItemInfo != null) { _win.TaskbarItemInfo.Overlay = img; _win.TaskbarItemInfo.Description = desc; }
    }

    // ---------- Configuració ----------
    public void ApplySettings()
    {
        Settings.IntervalMin = Math.Clamp(Settings.IntervalMin, 1, 1440);
        Loc.I.Language = Settings.Language;
        _timer.Interval = TimeSpan.FromMinutes(Settings.IntervalMin);
        _timer.Start();
        SetAutostart(Settings.Autostart);
        SettingsStore.Save(Settings);
    }

    void SetAutostart(bool on)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            if (on)
            {
                var exe = Environment.ProcessPath ?? "";
                key.SetValue("MailNotification", $"\"{exe}\"" + (Settings.StartMinimized ? " --hidden" : ""));
            }
            else key.DeleteValue("MailNotification", false);
        }
        catch { /* sense permisos: ignora */ }
    }

    // ---------- Comptes ----------
    public void SaveAccount(Account? existing, string email, string authType, string? secret, string notify)
    {
        if (existing == null)
        {
            var acc = new Account { Email = email, Notify = notify, AuthType = authType, Secret = SettingsStore.Protect(secret!) };
            Settings.Accounts.Add(acc);
            var vm = new AccountVm(acc);
            Accounts.Add(vm);
            _ = CheckOneAsync(vm, silent: true);
        }
        else
        {
            var vm = Accounts.First(a => a.Model == existing);
            existing.Email = email;
            existing.Notify = notify;
            existing.AuthType = authType;
            if (!string.IsNullOrEmpty(secret)) existing.Secret = SettingsStore.Protect(secret);
            vm.Known = new(); vm.Initialized = false;
            vm.Refresh();
            _ = CheckOneAsync(vm, silent: true);
        }
        SettingsStore.Save(Settings);
        UpdateBadge();
    }

    public void RemoveAccount(AccountVm vm)
    {
        Settings.Accounts.Remove(vm.Model);
        Accounts.Remove(vm);
        SettingsStore.Save(Settings);
        UpdateBadge();
        BuildTrayMenu();
    }

    public static void OpenGmail(Account acc) =>
        Process.Start(new ProcessStartInfo($"https://mail.google.com/mail/?authuser={Uri.EscapeDataString(acc.Email)}") { UseShellExecute = true });

    // ---------- Comprovació ----------
    public async Task CheckAllAsync()
    {
        if (_checking) return;
        _checking = true;
        try { await Task.WhenAll(Accounts.ToList().Select(a => CheckOneAsync(a))); }
        finally { _checking = false; }
    }

    async Task CheckOneAsync(AccountVm vm, bool silent = false)
    {
        vm.Checking = true;
        try
        {
            var r = await Task.Run(() => MailChecker.CheckAsync(vm.Model.Email, vm.Model.AuthType, SettingsStore.Unprotect(vm.Model.Secret), vm.Known));
            vm.Count = r.Uids.Count;
            vm.Error = null;
            if (vm.Initialized && !silent && r.Fresh.Count > 0 && vm.Model.Notify == "toast") Toast(vm, r.Fresh);
            vm.Known = r.Uids.ToHashSet();
            vm.Initialized = true;
        }
        catch (Exception ex)
        {
            vm.Error = MailChecker.ErrorKey(ex);
        }
        finally
        {
            vm.LastCheck = DateTime.Now;
            vm.Checking = false;
            UpdateBadge();
            BuildTrayMenu();
        }
    }

    void Toast(AccountVm vm, List<FreshMail> fresh)
    {
        if (_tray == null) return;
        _lastToast = vm;
        var title = fresh.Count > 1 ? $"{vm.Email} — {fresh.Count} {Loc.T("newMailMany")}" : $"{vm.Email} — {Loc.T("newMail")}";
        var body = string.Join("\n", fresh.Take(3).Select(m => $"{m.From}: {m.Subject}"));
        _tray.ShowBalloonTip(8000, title, body, WinForms.ToolTipIcon.Info);
    }

    // ---------- Safata i comptador ----------
    int TotalBadge() => Accounts.Where(a => a.Model.Notify == "badge" && a.Error == null).Sum(a => a.Count);

    void UpdateBadge()
    {
        if (_tray == null) return;
        var n = TotalBadge();
        var old = _trayIcon;
        _trayIcon = IconFactory.Tray(n);
        _tray.Icon = _trayIcon;
        old?.Dispose();
        var tip = n > 0 ? $"Mail Notification — {n} {Loc.T("unread")}" : "Mail Notification";
        _tray.Text = tip.Length > 63 ? tip[..63] : tip;
        OpenWindowOverlay(IconFactory.Overlay(n), n > 0 ? n.ToString() : "");
    }

    void BuildTrayMenu()
    {
        if (_tray == null) return;
        var m = new WinForms.ContextMenuStrip();
        m.Items.Add(Loc.T("trayOpen"), null, (_, _) => ShowWindow());
        m.Items.Add(Loc.T("trayCheck"), null, async (_, _) => await CheckAllAsync());
        if (Accounts.Count > 0) m.Items.Add(new WinForms.ToolStripSeparator());
        foreach (var a in Accounts.ToList())
        {
            var acc = a;
            m.Items.Add($"{acc.Email} ({(acc.Error != null ? "!" : acc.Count.ToString())})", null, (_, _) => OpenGmail(acc.Model));
        }
        m.Items.Add(new WinForms.ToolStripSeparator());
        m.Items.Add(Loc.T("trayQuit"), null, (_, _) => Quit());
        var old = _tray.ContextMenuStrip;
        _tray.ContextMenuStrip = m;
        old?.Dispose();
    }
}
