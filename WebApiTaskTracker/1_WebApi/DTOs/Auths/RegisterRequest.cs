using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password
)
{
    public class Validator : AbstractValidator<RegisterRequest>

    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}

