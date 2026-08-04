namespace WebApiTaskTracker.Utilities;

using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApiTaskTracker.Business.FluentErrors;

public static class ResultExtensions
{
    // This method converts a Result<T> object to a typed HTTP result, returning Ok<T> for success and a ProblemHttpResult for errors.
    public static Results<Ok<T>, ProblemHttpResult> ToTypedHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(result.Value);

        return ToProblemResult(result.Errors.FirstOrDefault());
    }

    // This method converts a Result<T> object to a typed HTTP result, returning CreatedAtRoute<T> for success and a ProblemHttpResult for errors.
    public static Results<CreatedAtRoute<T>, ProblemHttpResult> ToCreatedAtRouteResult<T>(
        this Result<T> result,
        string routeName,
        object? routeValues)
    {
        if (result.IsSuccess)
            return TypedResults.CreatedAtRoute(result.Value, routeName, routeValues);

        return ToProblemResult(result.Errors.FirstOrDefault());
    }

    // This method converts a Result object to a typed HTTP result, returning NoContent for success and a ProblemHttpResult for errors.
    public static Results<NoContent, ProblemHttpResult> ToTypedHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        return ToProblemResult(result.Errors.FirstOrDefault());
    }

    // Centralized mapper of FluentResults errors to the standard ProblemHttpResult (RFC 7807)
    private static ProblemHttpResult ToProblemResult(IError? error)
    {
        return error switch
        {
            NotFoundError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found"),

            ValidationError validationError => TypedResults.Problem(
                detail: validationError.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                extensions: new Dictionary<string, object?> { { "errors", validationError.Errors } }),

            TaskLimitExceededError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Task Limit Exceeded"),

            CategoryLimitExceededError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Category Limit Exceeded"),

            IdentityValidationError identityError => TypedResults.Problem(
                detail: identityError.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Identity Validation Error",
                extensions: new Dictionary<string, object?> { { "errors", identityError.Errors } }),

            ExceptionalError => TypedResults.Problem(
                detail: "A critical server or database error occurred. Please contact support.", // Not exposing internal exception details for security reasons.
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server or Database Error"),

            // In case we add roles or permissions in the future, we can handle UnauthorizedError here.
            // But generally authorization errors are handled by the ASP.NET Core middleware, so we might not need to handle them here.
            //UnauthorizedError => TypedResults.Problem(
            //    detail: error.Message,
            //    statusCode: StatusCodes.Status401Unauthorized,
            //    title: "Unauthorized"),

            _ => TypedResults.Problem(
                detail: error?.Message ?? "An unexpected error occurred.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error")
        };
    }
}