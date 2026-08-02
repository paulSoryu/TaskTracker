using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Auths;

public record ConfirmEmailRequest(
    string UserId,
    string Token
);
