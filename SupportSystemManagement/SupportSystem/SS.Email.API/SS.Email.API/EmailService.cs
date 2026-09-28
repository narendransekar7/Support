namespace SS.Email.API;

using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPass, string fromEmail, ILogger<EmailService> logger)
    {
        _logger = logger;
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPass = smtpPass;
        _fromEmail = fromEmail;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Support System", _fromEmail));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = body
        };

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Recipient address deliberately not logged (PII); the subject identifies the email type.
            _logger.LogError(ex, "Failed to send email \"{Subject}\" via {SmtpServer}:{SmtpPort}", subject, _smtpServer, _smtpPort);
            throw;
        }

        _logger.LogInformation("Sent email \"{Subject}\"", subject);
    }
}
