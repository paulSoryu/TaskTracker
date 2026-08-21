using FluentResults;

namespace TaskTracker.Business.Services.Seeding;

public interface IDataSeederService
{
    Task<Result> GenerateDefaultDataAsync(Guid userId, int taskAddAmount, int categoryAddAmount);
}