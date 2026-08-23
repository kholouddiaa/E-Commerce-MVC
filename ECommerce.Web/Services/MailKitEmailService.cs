using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerce.Web.Services;

public class MailKitEmailService(IOptions<EmailSettings> emailOptions) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailOptions.Value;

    public async Task SendHtmlEmailAsync(string recipientEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost) ||
            string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
        {
            throw new InvalidOperationException("Email settings are incomplete.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(
            _emailSettings.SmtpHost,
            _emailSettings.SmtpPort,
            SecureSocketOptions.StartTlsWhenAvailable);

        if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpUsername))
        {
            await smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
        }

        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
