using Microsoft.AspNetCore.Identity;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Services.Emails
{
    public class EmailSenderService : IEmailSender<UserEntity>
    {

        // Refactor this method to use an actual email sending service in a production environment.

        public Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"====== [EMAIL SIMULATION] ======");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Reset Code: {resetCode}");
            Console.WriteLine($"===============================");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        public Task SendConfirmationLinkAsync(UserEntity user, string email, string confirmationLink)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"====== [EMAIL SIMULATION] ======");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Confirmation Link: {confirmationLink}");
            Console.WriteLine($"===============================");
            Console.ResetColor();
            return Task.CompletedTask;
        }
        public Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink) => Task.CompletedTask;
    }
}
