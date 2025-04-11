using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendEmailConfirmationAsync(string to, string confirmationLink);
        Task<bool> SendPasswordResetAsync(string to, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                
                // Validate SMTP settings
                if (string.IsNullOrEmpty(smtpSettings["Server"]) || 
                    string.IsNullOrEmpty(smtpSettings["Username"]) || 
                    string.IsNullOrEmpty(smtpSettings["Password"]))
                {
                    _logger.LogError("SMTP settings are not properly configured");
                    return false;
                }

                var smtpServer = smtpSettings["Server"];
                var smtpPort = int.Parse(smtpSettings["Port"]);
                var smtpUsername = smtpSettings["Username"];
                var smtpPassword = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                var requireAuth = bool.Parse(smtpSettings["RequireAuthentication"] ?? "true");

                using var client = new SmtpClient(smtpServer, smtpPort);
                
                if (requireAuth)
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                }
                
                client.EnableSsl = enableSsl;

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUsername, fromName ?? "Smart Inventory Management System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailConfirmationAsync(string to, string confirmationLink)
        {
            var subject = "Confirm your email";
            var body = $@"
                <h2>Welcome to Smart Inventory Management System</h2>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>If you did not request this confirmation, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendPasswordResetAsync(string to, string resetLink)
        {
            var subject = "Reset your password";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password. Click the link below to proceed:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this reset, please ignore this email.</p>
                <p>This link will expire in 1 hour.</p>";

            return await SendEmailAsync(to, subject, body);
        }
    }
} 