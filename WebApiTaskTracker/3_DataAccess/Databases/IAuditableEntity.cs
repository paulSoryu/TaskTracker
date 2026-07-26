namespace WebApiTaskTracker.DataAccess.Databases;

// This interface is used to mark entities that should have auditing information, such as the creation date. It can be implemented by any entity that requires this information.
// The CreatedAt property will be automatically set when SaveChangesAsync() is called.
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
}
