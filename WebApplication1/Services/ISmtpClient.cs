using System.Net.Mail;

namespace WebApplication1.Services
{
    public interface ISmtpClient
    {
        Task SendMailAsync(MailMessage message, CancellationToken cancellationToken = default);
    }
} 