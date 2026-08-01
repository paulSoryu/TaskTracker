using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Auth;

public record ChangePasswordRequest(
    string CurrentPassword, 
    string NewPassword
)
{
    public class Validator : AbstractValidator<ChangePasswordRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty();
        }
    }
}
