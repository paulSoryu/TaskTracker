namespace WebApiTaskTracker.Utilities;

using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using WebApiTaskTracker.Business.FluentErrors;

public static class ResultExtensions
{
    public static Results<Ok<T>, ProblemHttpResult> ToTypedHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(result.Value);

        var error = result.Errors.FirstOrDefault();

        return error switch
        {
            NotFoundError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found"),

            ValidationError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                extensions: new Dictionary<string, object?>
                {
                    { "errors", result.Errors.Select(e => e.Message) }
                }),

            TaskLimitExceededError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Task Limit Exceeded"),

            // Ideally, TaskLimitExceededError and CategoryLimitExceededError should be handled in one generic way to not duplicate code, but for now, we handle them separately.
            CategoryLimitExceededError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Category Limit Exceeded"),

            IdentityValidationError identityError => TypedResults.Problem(
                detail: identityError.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                extensions: new Dictionary<string, object?>
                { 
                    { "errors", identityError.Errors }
                }),

            // In case we will add other roles like Admin, Moderator, etc., we can handle them here.
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

    public static Results<NoContent, ProblemHttpResult> ToTypedHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        var error = result.Errors.FirstOrDefault();

        return error switch
        {
            NotFoundError => TypedResults.Problem(detail: error.Message, statusCode: 404, title: "Not Found"),
            ValidationError => TypedResults.Problem(detail: error.Message, statusCode: 400, title: "Validation Error"),
            TaskLimitExceededError => TypedResults.Problem(detail: error.Message, statusCode: 400, title: "Task Limit Exceeded"),
            CategoryLimitExceededError => TypedResults.Problem(detail: error.Message, statusCode: 400, title: "Category Limit Exceeded"),
            _ => TypedResults.Problem(detail: error?.Message, statusCode: 500, title: "Server Error")
        };
    }

    public static Results<CreatedAtRoute<T>, ProblemHttpResult> ToCreatedAtRouteResult<T>(
        this Result<T> result,
        string routeName,
        object? routeValues)
    {
        if (result.IsSuccess)
            return TypedResults.CreatedAtRoute(result.Value, routeName, routeValues);

        var error = result.Errors.FirstOrDefault();

        return error switch
        {
            NotFoundError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found"),

            ValidationError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                extensions: new Dictionary<string, object?> { { "errors", result.Errors.Select(e => e.Message) } }),

            TaskLimitExceededError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Task Limit Exceeded"),

            CategoryLimitExceededError => TypedResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Category Limit Exceeded"),

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
