using WebApplication1.Services;

namespace WebApplication1.Tests.Services
{
    public class TestEmailService : IEmailService
    {
        public bool EmailSent { get; private set; }
        public string LastEmailTo { get; private set; }
        public string LastEmailSubject { get; private set; }
        public string LastEmailBody { get; private set; }
        public string LastConfirmationLink { get; private set; }
        public string LastResetLink { get; private set; }

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            EmailSent = true;
            LastEmailTo = to;
            LastEmailSubject = subject;
            LastEmailBody = body;
            return Task.FromResult(true);
        }

        public Task<bool> SendEmailConfirmationAsync(string to, string confirmationLink)
        {
            LastConfirmationLink = confirmationLink;
            return SendEmailAsync(to, "Confirm your email", $"Please confirm your email by clicking this link: {confirmationLink}");
        }

        public Task<bool> SendPasswordResetAsync(string to, string resetLink)
        {
            LastResetLink = resetLink;
            return SendEmailAsync(to, "Reset your password", $"Please reset your password by clicking this link: {resetLink}");
        }
    }
} 