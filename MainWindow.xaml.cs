using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MailNotification;

public partial class MainWindow : Window
{
    readonly Controller _c = Controller.Instance;
    bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        List.ItemsSource = _c.Accounts;
        _c.Accounts.CollectionChanged += (_, _) => UpdateEmpty();
        UpdateEmpty();

        var s = _c.Settings;
        foreach (ComboBoxItem it in LangBox.Items) if ((string)it.Tag == s.Language) LangBox.SelectedItem = it;
        IntervalBox.Text = s.IntervalMin.ToString();
        AutostartBox.IsChecked = s.Autostart;
        StartMinBox.IsChecked = s.StartMinimized;
        TrayBox.IsChecked = s.MinimizeToTray;
        ClientIdBox.Text = s.OAuthClientId;
        ClientSecretBox.Password = SettingsStore.Unprotect(s.OAuthClientSecret);
        _loading = false;
    }

    void UpdateEmpty() => EmptyText.Visibility = _c.Accounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    // Minimitzar o tancar envia l'app a la safata del sistema.
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized && _c.CanHideToTray) Hide();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_c.CanHideToTray) { e.Cancel = true; Hide(); }
        else _c.Quit();
        base.OnClosing(e);
    }

    static AccountVm Vm(object sender) => (AccountVm)((FrameworkElement)sender).DataContext;

    void Add_Click(object sender, RoutedEventArgs e) => ShowDialog(null);
    void Edit_Click(object sender, RoutedEventArgs e) => ShowDialog(Vm(sender).Model);
    void Open_Click(object sender, RoutedEventArgs e) => Controller.OpenGmail(Vm(sender).Model);
    void Check_Click(object sender, RoutedEventArgs e) => _ = _c.CheckAllAsync();

    void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, Loc.T("confirmRemove"), "Mail Notification", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            _c.RemoveAccount(Vm(sender));
    }

    void ShowDialog(Account? acc) => new AccountDialog(acc, _c.Settings.Accounts) { Owner = this }.ShowDialog();

    // ---------- Configuració ----------
    void Lang_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || LangBox.SelectedItem is not ComboBoxItem it) return;
        _c.Settings.Language = (string)it.Tag;
        _c.ApplySettings();
    }

    void Interval_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _c.Settings.IntervalMin = int.TryParse(IntervalBox.Text, out var n) ? n : _c.Settings.IntervalMin;
        _c.ApplySettings();
        IntervalBox.Text = _c.Settings.IntervalMin.ToString();
    }

    void OAuth_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _c.Settings.OAuthClientId = ClientIdBox.Text.Trim();
        var secret = ClientSecretBox.Password.Trim();
        _c.Settings.OAuthClientSecret = secret.Length > 0 ? SettingsStore.Protect(secret) : "";
        _c.ApplySettings();
    }

    void Option_Click(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _c.Settings.Autostart = AutostartBox.IsChecked == true;
        _c.Settings.StartMinimized = StartMinBox.IsChecked == true;
        _c.Settings.MinimizeToTray = TrayBox.IsChecked == true;
        _c.ApplySettings();
    }
}
