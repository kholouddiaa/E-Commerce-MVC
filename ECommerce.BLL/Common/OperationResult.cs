namespace ECommerce.BLL.Common;

public class OperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public string? ErrorPropertyName { get; init; }

    public static OperationResult Success()
    {
        return new OperationResult { Succeeded = true };
    }

    public static OperationResult Failure(string errorMessage, string? errorPropertyName = null)
    {
        return new OperationResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            ErrorPropertyName = errorPropertyName
        };
    }
}
