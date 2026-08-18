using FluentResults;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Tasks;

namespace TaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<PagedResult<TaskView>> GetAllAsync(FilterTasksQuery filterQuery, SortTasksQuery sortQuery, PaginateTasksQuery paginateQuery);
    Task<Result<TaskView>> GetByIdAsync(Guid id);
    Task<Result<int>> GetPageById(Guid id, SortTasksQuery query, int pageSize);
    Task<Result<TaskView>> CreateAsync(SaveTaskCommand command, SortTasksQuery query, Guid userId);
    Task<Result> UpdateAsync(SaveTaskCommand command);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> MoveAsync(MoveTaskCommand command, SortTasksQuery query);
    Task<Result> CreateDefaultTasksAsync(Guid userId, Dictionary<string, Guid> categoryIdsByName);
    Task DeleteAllByUserIdAsync(Guid userId);
}