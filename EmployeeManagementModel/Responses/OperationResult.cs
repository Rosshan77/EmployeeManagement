namespace EmployeeManagementModel.Responses
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }
    }
}