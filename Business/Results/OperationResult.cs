namespace Business.Results
{
    public class OperationResult
    {
        public bool IsSuccess { get; }
        public bool IsNotFound { get; }
        public bool IsConflict { get; }
        public bool IsFailure { get; }
        public string? ErrorMessage { get; }

        // Protected ctor
        protected OperationResult(bool success, bool notFound, bool conflict, bool failure, string? message = null)
        {
            IsSuccess = success;
            IsNotFound = notFound;
            IsConflict = conflict;
            IsFailure = failure;
            ErrorMessage = message;
        }

        public static OperationResult Success() => new(true, false, false, false);
        public static OperationResult NotFound() => new(false, true, false, false);
        public static OperationResult Conflict(string msg) => new(false, false, true, false, msg);
        public static OperationResult Failure(string msg) => new(false, false, false, true, msg);
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Value { get; }

        private OperationResult(T value)
            : base(true, false, false, false)
        {
            Value = value;
        }

        private OperationResult(bool success, bool notFound, bool conflict, bool failure, T? value, string? message)
            : base(success, notFound, conflict, failure, message)
        {
            Value = value;
        }

        public static OperationResult<T> Success(T value) => new(value);
        public static new OperationResult<T> NotFound() => new(false, true, false, false, default, null);
        public static OperationResult<T> Conflict(string msg) => new(false, false, true, false, default, msg);
        public static OperationResult<T> Failure(string msg) => new(false, false, false, true, default, msg);
    }
}
