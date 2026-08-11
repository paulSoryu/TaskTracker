using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using WebApiTaskTracker.Business.Extensions;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Services.Auths;
using WebApiTaskTracker.Business.Services.Categories;
using WebApiTaskTracker.Business.Services.Tasks;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Services.Users;

public class UserCoordinator(
    IUserService userService,
    IAuthService authService,
    ICategoryService categoryService,
    ITaskService taskService,
    UserManager<UserEntity> userManager,
    IEmailSender<UserEntity> emailSender,
    TaskTrackerDbContext db
    ) : IUserCoordinator
{
    const string frontendBaseUrl = "http://localhost:3000/index.html";

    public async Task<Result> RegisterAndCreateDefaultDataAsync(string email, string password)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // this CreateAsync method doesn't write anything into DB as ASP.NET Identity already does this in RegisterAsync
            // but if we ever remove Identity, writing into DB should be here, and password hashing should be in RegisterAsync
            // it is also async just for the sake of consistency and easier changes later on
            var createResult = await userService.CreateAsync(email);
            if (createResult.IsFailed)
                return createResult.ToResult();

            var user = createResult.Value;

            var registerResult = await authService.RegisterAsync(password, user);
            if (registerResult.IsFailed)
                return registerResult.ToResult();

            Guid userId = registerResult.Value;

            var createCategoriesResult = await categoryService.CreateDefaultCategoriesAsync(userId);
            if (createCategoriesResult.IsFailed)
                return createCategoriesResult.ToResult();

            var categoriesDictionary = createCategoriesResult.Value;

            var createTasksResult = await taskService.CreateDefaultTasksAsync(userId, categoriesDictionary);
            if (createTasksResult.IsFailed)
                return createTasksResult;

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("Registration failed due to an internal database error.", ex));
        }
    }

    public async Task<Result> SendEmailConfirmationLetterAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Fail(new NotFoundError("User", email));

        var generateTokenResult = await authService.GenerateConfirmEmailTokenAsync(user);
        if (generateTokenResult.IsFailed)
            return generateTokenResult.ToResult();

        var encodedToken = generateTokenResult.Value;

        var confirmationLink = $"{frontendBaseUrl}?confirmEmail=1&userId={user.Id}&encodedToken={encodedToken}";

        //var sendResult = 
        await emailSender.SendConfirmationLinkAsync(user, email, confirmationLink);
        //if (sendResult.IsFailed)
        //    return sendResult.ToResult();

        return Result.Ok();
    }

    public async Task<Result> SendEmailChangeLetterAsync(string currentEmail, string newEmail, string password)
    {
        var user = await userManager.FindByEmailAsync(currentEmail);
        if (user == null)
            return Result.Fail(new NotFoundError("User", currentEmail));

        var checkPasswordResult = await authService.VerifyPasswordAsync(user, password);
        if (checkPasswordResult.IsFailed)
            return checkPasswordResult;

        var generateTokenResult = await authService.GenerateChangeEmailTokenAsync(user, newEmail);
        if (generateTokenResult.IsFailed)
            return generateTokenResult.ToResult();

        var encodedToken = generateTokenResult.Value;
        var confirmationLink = $"{frontendBaseUrl}?confirmChangeEmail=1&newEmail={newEmail}&encodedToken={encodedToken}";

        //var sendResult = 
        await emailSender.SendConfirmationLinkAsync(user, newEmail, confirmationLink);
        //if (sendResult.IsFailed)
        //    return sendResult.ToResult();

        return Result.Ok();
    }

    public async Task<Result> DeleteUserAndDataAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Fail(new NotFoundError("User", email));

        var checkPasswordResult = await authService.VerifyPasswordAsync(user, password);
        if (checkPasswordResult.IsFailed)
            return checkPasswordResult;

        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            await taskService.DeleteAllByUserIdAsync(user.Id);
            await categoryService.DeleteAllByUserIdAsync(user.Id);

            var deleteUserResult = await userService.DeleteAsync(user.Id.ToString());
            if (!deleteUserResult.IsFailed)
                return deleteUserResult;

            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("Registration failed due to an internal database error.", ex));
        }
    }
}
