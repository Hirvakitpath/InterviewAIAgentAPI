using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

public class GmailServiceHelper
{
    private readonly IWebHostEnvironment _env;

    public GmailServiceHelper(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<GmailService> GetGmailServiceAsync()
    {
        var credentialPath = Path.Combine(_env.WebRootPath, "credential.json");

        using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);
        var tokenPath = Path.Combine(_env.ContentRootPath, "token.json");

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new[] { GmailService.Scope.GmailSend },
            "user",
            CancellationToken.None,
            new FileDataStore(tokenPath, true)
        );

        return new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Gmail API .NET Core"
        });
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var service = await GetGmailServiceAsync();

        var message = new Google.Apis.Gmail.v1.Data.Message
        {
            Raw = EncodeToBase64(to, subject, body)
        };

        await service.Users.Messages.Send(message, "me").ExecuteAsync();
    }

    private string EncodeToBase64(string to, string subject, string body)
    {
        var emailText = $"To: {to}\r\n" +
                        $"Subject: {subject}\r\n" +
                        "Content-Type: text/html; charset=utf-8\r\n\r\n" +
                        $"{body}";

        var bytes = System.Text.Encoding.UTF8.GetBytes(emailText);
        return System.Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}
