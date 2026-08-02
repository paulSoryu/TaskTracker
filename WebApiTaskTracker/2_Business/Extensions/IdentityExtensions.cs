using FluentResults;
using Microsoft.AspNetCore.Identity;
using WebApiTaskTracker.Business.FluentErrors;

namespace WebApiTaskTracker.Business.Extensions;

// A static class that provides extension methods for converting IdentityResult to FluentResults Result, as it they are not compatible by default.
public static class IdentityExtensions
{
    public static Result ToFluentResult(this IdentityResult identityResult)
    {
        return identityResult.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError("Identity validation failed.", identityResult.Errors));
    }
}