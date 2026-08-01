using FluentResults;
using Microsoft.AspNetCore.Identity;
using WebApiTaskTracker.Business.FluentErrors;

namespace WebApiTaskTracker.Business.Extensions;

public static class IdentityExtensions
{
    public static Result ToFluentResult(this IdentityResult identityResult)
    {
        return identityResult.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError(identityResult.Errors));
    }
}