namespace WebApiTaskTracker.Utilities;

using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        {
            return TypedResults.NoContent();
        }

        var error = result.Errors.FirstOrDefault();

        return error switch
        {
            NotFoundError => TypedResults.Problem(detail: error.Message, statusCode: 404, title: "Not Found"),
            ValidationError => TypedResults.Problem(detail: error.Message, statusCode: 400, title: "Validation Error"),
            TaskLimitExceededError => TypedResults.Problem(detail: error.Message, statusCode: 400, title: "Task Limit Exceeded"),
            _ => TypedResults.Problem(detail: error?.Message, statusCode: 500, title: "Server Error")
        };
    }

    public static Results<CreatedAtRoute<T>, ProblemHttpResult> ToCreatedAtRouteResult<T>(
        this Result<T> result,
        string routeName,
        object? routeValues)
    {
        // Если успех — возвращаем строго типизированный 201 CreatedAtRoute
        if (result.IsSuccess)
        {
            return TypedResults.CreatedAtRoute(result.Value, routeName, routeValues);
        }

        // Если ошибка — маппим в Problem Details (400, 401, 404 или 500)
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
