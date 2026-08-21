namespace TaskTracker.Shared.Constants;

public class TaskConstraints
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 50;
    public const int DescriptionMaxLength = 2000;

    // Used in TaskService to limit the number of tasks a user can create based on their email confirmation status
    public const int MaxTasksForUnconfirmedEmail = 20;
    public const int MaxTasksForConfirmedEmail = 1000;
}

public class UserConstraints
{
    // User constraints in case we'll need to add any specific constraints for user properties in the future
}

public class CategoryConstraints
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 50;

    // Used in CategoryService to limit the number of categories a user can create based on their email confirmation status
    public const int MaxCategoriesForUnconfirmedEmail = 10;
    public const int MaxCategoriesForConfirmedEmail = 100;
}

public class PaginationConstraints
{
    public const int PageMinSize = 10; 
    public const int PageMaxSize = 100;
}

public class RoleConstraints
{
    public static readonly Guid AdminRoleId = Guid.Parse("CAF8ABC3-174D-4E54-B5B2-950CB55E748F");
}