namespace WebApiTaskTracker.WebApi.DTOs.Auth;

public record ConfirmChangeEmailDto(string NewEmail, string Token);
