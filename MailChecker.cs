using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;

namespace MailNotification;

public record FreshMail(string From, string Subject);
public record CheckResult(List<uint> Uids, List<FreshMail> Fresh);

public static class MailChecker
{
    /// <summary>Compta els correus sense llegir de la safata d'entrada i detecta els que no eren a <paramref name="known"/>.</summary>
    public static async Task<CheckResult> CheckAsync(string email, string authType, string secret, ISet<uint> known)
    {
        using var client = new ImapClient { Timeout = 30000 };
        await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
        if (authType == "oauth")
        {
            var token = await OAuthService.GetAccessTokenAsync(secret);
            await client.AuthenticateAsync(new SaslMechanismOAuth2(email, token));
        }
        else
            await client.AuthenticateAsync(email, secret.Replace(" ", ""));

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly);
        var ids = await inbox.SearchAsync(SearchQuery.NotSeen);

        var fresh = new List<FreshMail>();
        var newIds = ids.Where(i => !known.Contains(i.Id)).TakeLast(5).ToList();
        if (newIds.Count > 0)
        {
            foreach (var s in await inbox.FetchAsync(newIds, MessageSummaryItems.Envelope))
            {
                var from = s.Envelope?.From?.Mailboxes.FirstOrDefault();
                fresh.Add(new FreshMail(from?.Name is { Length: > 0 } n ? n : from?.Address ?? "", s.Envelope?.Subject ?? ""));
            }
        }
        await client.DisconnectAsync(true);
        return new CheckResult(ids.Select(i => i.Id).ToList(), fresh);
    }

    public static string ErrorKey(Exception ex) =>
        ex is AuthenticationException || ex is Google.Apis.Auth.OAuth2.Responses.TokenResponseException ? "errAuth" : "errGeneric";
}
