namespace TaskTracker.Api.DTOs.Admin;

public record SeedDemoDataRequest(
    int TaskAddAmount,
    int CategoryAddAmount
);