namespace WebApiTaskTracker.Utilities;

public class TaskConstraints
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 50;
    public const int DescriptionMaxLength = 100;
    public const int PriorityMinValue = 1;
    public const int PriorityMaxValue = 3;
}

public class UserConstraints
{
    // User constraints in case you want to add any specific constraints for user properties in the future
}

public class CategoryConstraints
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 50;
}
