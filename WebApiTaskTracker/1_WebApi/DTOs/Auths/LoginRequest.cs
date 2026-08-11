using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Auths;

public record LoginRequest(
    string Email, 
    string Password,
    bool RememberMe
)
{
    public class Validator : AbstractValidator<LoginRequest>
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
