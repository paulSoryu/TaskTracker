namespace WebApiTaskTracker.WebApi.DTOs.Auths;

public record ConfirmChangeEmailRequest(
    string NewEmail, 
    string Token
);
