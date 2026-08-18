using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Services.Emails;

public interface IEmailSenderService<T>
{
    Task SendConfirmationLinkAsync(UserEntity user, string email, string confirmationLink);
    Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink);
    Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode);
}