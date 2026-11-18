using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;

namespace MailNotification;

/// <summary>Inici de sessió amb Google (OAuth 2.0, aplicació d'escriptori) per accedir a IMAP amb XOAUTH2.</summary>
public static class OAuthService
{
    const string Scope = "https://mail.google.com/";

    sealed class HintFlow : GoogleAuthorizationCodeFlow
    {
        public string? Hint;
        public HintFlow(Initializer init) : base(init) { }
        public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
        {
            var url = (GoogleAuthorizationCodeRequestUrl)base.CreateAuthorizationCodeRequest(redirectUri);
            url.LoginHint = Hint;
            url.Prompt = "consent"; // garanteix que Google retorna un refresh token
            return url;
        }
    }

    static GoogleAuthorizationCodeFlow.Initializer Init() => new()
    {
        ClientSecrets = new ClientSecrets
        {
            ClientId = Controller.Instance.Settings.OAuthClientId,
            ClientSecret = SettingsStore.Unprotect(Controller.Instance.Settings.OAuthClientSecret),
        },
        Scopes = new[] { Scope },
        DataStore = new NullDataStore(),
    };

    public static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Controller.Instance.Settings.OAuthClientId)
        && !string.IsNullOrEmpty(Controller.Instance.Settings.OAuthClientSecret);

    /// <summary>Obre el navegador, deixa que l'usuari iniciï sessió i retorna el refresh token.</summary>
    public static async Task<string> AuthorizeAsync(string email)
    {
        var flow = new HintFlow(Init()) { Hint = email };
        var app = new AuthorizationCodeInstalledApp(flow, new LocalServerCodeReceiver());
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var cred = await app.AuthorizeAsync("user", cts.Token);
        return cred.Token.RefreshToken
            ?? throw new InvalidOperationException("Google no ha retornat cap refresh token");
    }

    public static async Task<string> GetAccessTokenAsync(string refreshToken)
    {
        var flow = new GoogleAuthorizationCodeFlow(Init());
        var cred = new UserCredential(flow, "user", new TokenResponse { RefreshToken = refreshToken });
        return await cred.GetAccessTokenForRequestAsync();
    }
}
