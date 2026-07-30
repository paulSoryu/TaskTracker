namespace WebApiTaskTracker.Utilities
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() { }

        public EntityNotFoundException(string message)
            : base(message) { }

        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException() { }

        public EntityAlreadyExistsException(string message)
            : base(message) { }

        public EntityAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidDateException : Exception
    {
        public InvalidDateException() { }

        public InvalidDateException(string message)
            : base(message) { }

        public InvalidDateException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
