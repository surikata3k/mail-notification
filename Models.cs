using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MailNotification;

public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = "";
    /// <summary>"none", "badge" o "toast"</summary>
    public string Notify { get; set; } = "toast";
    /// <summary>"password" (contrasenya d'aplicació) o "oauth" (inici de sessió amb Google).</summary>
    public string AuthType { get; set; } = "password";
    /// <summary>Contrasenya d'aplicació o refresh token d'OAuth, xifrats amb DPAPI (Base64).</summary>
    public string Secret { get; set; } = "";
}

public class AppSettings
{
    public string Language { get; set; } = "ca";
    public int IntervalMin { get; set; } = 5;
    public bool Autostart { get; set; }
    public bool StartMinimized { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public string OAuthClientId { get; set; } = "";
    /// <summary>Client secret xifrat amb DPAPI.</summary>
    public string OAuthClientSecret { get; set; } = "";
    public List<Account> Accounts { get; set; } = new();
}

public static class SettingsStore
{
    static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MailNotification");
    static string FilePath => Path.Combine(Dir, "settings.json");

    public static AppSettings Load()
    {
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new(); }
        catch { return new(); }
    }

    public static void Save(AppSettings s)
    {
        Directory.CreateDirectory(Dir);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static string Protect(string plain) =>
        Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), null, DataProtectionScope.CurrentUser));

    public static string Unprotect(string secret)
    {
        try { return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(secret), null, DataProtectionScope.CurrentUser)); }
        catch { return ""; }
    }
}
