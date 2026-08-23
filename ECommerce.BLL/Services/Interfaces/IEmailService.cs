namespace ECommerce.BLL.Services.Interfaces;

public interface IEmailService
{
    Task SendHtmlEmailAsync(string recipientEmail, string subject, string htmlBody);
}
