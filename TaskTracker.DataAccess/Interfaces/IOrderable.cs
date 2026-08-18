namespace TaskTracker.DataAccess.Interfaces;


// This interface is used to mark entities which can be reordered via DbContextExtensions
public interface IOrderable
{
    int Position { get; set; }
}