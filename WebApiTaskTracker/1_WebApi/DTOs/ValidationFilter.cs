using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs;

// Validation filter for minimal APIs that uses FluentValidation to validate the request body of generic type.
// If the validation fails, it returns a 409 Conflict response with the validation errors.
// If the request body is null or has an invalid format, it also returns a 400 Bad Request response.
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.FirstOrDefault(x => x is T) as T;

        if (argument is null)
            return TypedResults.BadRequest("The request body cannot be empty or have an invalid format.");

        var validationResult = await _validator.ValidateAsync(argument);

        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        

        return await next(context);
    }
}

// Extension method to add the validation filter to a route handler builder for a specific type.
public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}