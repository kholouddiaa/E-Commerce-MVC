namespace ECommerce.BLL.Common;

public class OperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public static OperationResult Success()
    {
        return new OperationResult { Succeeded = true };
    }

    public static OperationResult Failure(string errorMessage)
    {
        return new OperationResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}
