using Microsoft.AspNetCore.Identity.UI.Services;

namespace AmethystScreen.Services
{
    public class MockEmailSenderService : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }

}
