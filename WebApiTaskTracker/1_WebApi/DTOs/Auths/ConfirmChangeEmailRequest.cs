namespace WebApiTaskTracker.WebApi.DTOs.Auth;

public record ConfirmChangeEmailRequest(
    string NewEmail, 
    string Token
);
