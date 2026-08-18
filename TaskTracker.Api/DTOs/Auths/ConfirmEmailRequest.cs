using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;

namespace TaskTracker.Api.DTOs.Auths;

public record ConfirmEmailRequest(
    string UserId,
    string EncodedToken
)
{
    public class Validator : AbstractValidator<ConfirmEmailRequest>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId cannot be empty.");
            RuleFor(x => x.EncodedToken)
                .NotEmpty().WithMessage("Token cannot be empty.")
                .Must(BeAValidBase64Url).WithMessage("The provided string is not a valid Base64Url.");
        }
        private bool BeAValidBase64Url(string token)
        {
            try
            {
                var decodedBytes = WebEncoders.Base64UrlDecode(token);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
