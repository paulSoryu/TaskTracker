using FluentResults;
using WebApiTaskTracker.Business.Models;
using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<PagedResult<TaskView>> GetAllAsync(FilterTasksQuery filterQuery, SortTasksQuery sortQuery, PaginateTasksQuery paginateQuery);
    Task<Result<TaskView>> GetByIdAsync(Guid id);
    Task<Result<TaskView>> CreateAsync(SaveTaskCommand command, Guid userId);
    Task<Result> UpdateAsync(SaveTaskCommand command);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ReorderTaskAsync(MoveTaskCommand command);
    Task<Result> ResetAllPositionsAsync(MoveTaskCommand command, SortTasksQuery query);
}