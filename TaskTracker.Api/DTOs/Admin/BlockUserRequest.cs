using FluentValidation;
using TaskTracker.Api.DTOs.Auths;

namespace TaskTracker.Api.DTOs.Admin;

public record BlockUserRequest(
    DateOnly? Until
);