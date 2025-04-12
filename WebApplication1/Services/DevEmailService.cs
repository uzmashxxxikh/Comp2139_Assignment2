using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using WebApplication1.Services;

namespace WebApplication1.Services
{
    public class DevEmailService : IEmailService
    {
        private readonly ILogger<DevEmailService> _logger;

        public DevEmailService(ILogger<DevEmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Manually create a test Ethereal account
                var username = "magdalen.bauch@ethereal.email";
                var password = "$$Am13811381$$";
                var smtpHost = "smtp.ethereal.email";
                var smtpPort = 587;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Smart Inventory (Dev)", username));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("📨 Dev email sent to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send dev email to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailConfirmationAsync(string to, string confirmationLink)
        {
            var subject = "Confirm your email (DEV)";
            var body = $@"
                <h2>Welcome to Dev Mode</h2>
                <p>Please confirm your email:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>This is a development environment email.</p>";
            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendPasswordResetAsync(string to, string resetLink)
        {
            var subject = "Reset your password (DEV)";
            var body = $@"
                <h2>Reset Password Request</h2>
                <p>To reset your password, click the link below:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This is a test email sent from the development environment.</p>";
            return await SendEmailAsync(to, subject, body);
        }
    }
}
