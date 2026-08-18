using Microsoft.AspNetCore.Identity;
using TaskTracker.Business.Services.Emails;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Api.Utilities;

public class IdentityEmailSenderProxy(IEmailSenderService<UserEntity> customSender) : IEmailSender<UserEntity>
{
    public Task SendConfirmationLinkAsync(UserEntity user, string email, string confirmationLink)
        => customSender.SendConfirmationLinkAsync(user, email, confirmationLink);

    public Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
        => customSender.SendPasswordResetLinkAsync(user, email, resetLink);

    public Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode)
        => customSender.SendPasswordResetCodeAsync(user, email, resetCode);
}