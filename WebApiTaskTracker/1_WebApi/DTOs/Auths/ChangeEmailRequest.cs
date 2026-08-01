using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Auth;

public record ChangeEmailRequest(
    string Password, 
    string NewEmail
)
{
    public class Validator : AbstractValidator<ChangeEmailRequest>
    {
        public Validator()
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
