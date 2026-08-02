using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Auths;

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
