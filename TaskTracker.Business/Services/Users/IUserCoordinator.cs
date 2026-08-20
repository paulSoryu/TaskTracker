using FluentResults;

namespace TaskTracker.Business.Services.Users;

public interface IUserCoordinator
{
    Task<Result> RegisterAndCreateDefaultDataAsync(string email, string password);
    Task<Result> SendEmailConfirmationLetterAsync(string email);
    Task<Result> SendEmailChangeLetterAsync(string currentEmail, string newEmail, string password);
    Task<Result> DeleteUserAndDataAsync(string email, string password);
    Task<Result> DeleteUserAndDataByAdminAsync(Guid id, string reason);
    Task<Result> SendDeletionWarningLetterAsync(Guid id, string reason);

}
