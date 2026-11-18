using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MailNotification;

public partial class AccountDialog : Window
{
    readonly Account? _existing;
    readonly List<Account> _all;

    public AccountDialog(Account? existing, List<Account> all)
    {
        InitializeComponent();
        _existing = existing;
        _all = all;
        TitleText.Text = Loc.T(existing == null ? "addAccount" : "edit");
        Title = TitleText.Text;
        EmailBox.Text = existing?.Email ?? "";
        Select(NotifyBox, existing?.Notify ?? "toast");
        Select(MethodBox, existing?.AuthType ?? "password");
        UpdateMethod();
    }

    static void Select(ComboBox box, string tag)
    {
        foreach (ComboBoxItem it in box.Items) if ((string)it.Tag == tag) box.SelectedItem = it;
    }

    string Method => (string)((ComboBoxItem)MethodBox.SelectedItem).Tag;

    void Method_Changed(object sender, SelectionChangedEventArgs e) => UpdateMethod();

    void UpdateMethod()
    {
        if (PassPanel == null) return;
        var oauth = Method == "oauth";
        PassPanel.Visibility = oauth ? Visibility.Collapsed : Visibility.Visible;
        OAuthHelp.Visibility = oauth ? Visibility.Visible : Visibility.Collapsed;
    }

    void ShowError(string key, string? detail = null)
    {
        ErrorText.Text = Loc.T(key) + (detail != null ? "\n(" + detail + ")" : "");
        ErrorText.Visibility = Visibility.Visible;
    }

    async void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        var email = EmailBox.Text.Trim().ToLowerInvariant();
        var pass = PassBox.Password;
        var notify = (string)((ComboBoxItem)NotifyBox.SelectedItem).Tag;
        var method = Method;
        var oauth = method == "oauth";

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { ShowError("errEmail"); return; }
        if (_all.Any(a => a.Email == email && a != _existing)) { ShowError("errDup"); return; }
        if (!oauth && _existing == null && pass.Length == 0) { ShowError("errPass"); return; }
        if (oauth && !OAuthService.IsConfigured) { ShowError("errOAuthMissing"); return; }

        // Credencial nova: contrasenya escrita o refresh token d'una autorització nova. Si és buida, es manté l'actual.
        string? newSecret = null;
        var changed = _existing == null || _existing.Email != email || _existing.AuthType != method || (!oauth && pass.Length > 0);
        SaveBtn.IsEnabled = false;
        SaveBtn.Content = Loc.T("verifying");
        try
        {
            if (oauth && changed)
                newSecret = await OAuthService.AuthorizeAsync(email); // obre el navegador
            else if (pass.Length > 0) newSecret = pass;

            if (changed)
            {
                var secret = newSecret ?? SettingsStore.Unprotect(_existing!.Secret);
                await Task.Run(() => MailChecker.CheckAsync(email, method, secret, new HashSet<uint>()));
            }
        }
        catch (Exception ex)
        {
            ShowError(MailChecker.ErrorKey(ex), ex.Message);
            SaveBtn.IsEnabled = true;
            SaveBtn.Content = Loc.T("save");
            return;
        }

        Controller.Instance.SaveAccount(_existing, email, method, newSecret, notify);
        DialogResult = true;
    }
}
