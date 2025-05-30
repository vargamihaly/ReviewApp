namespace ReviewApp.Application.Results;

public enum SubmitReviewFailureType
{
    ProductNotFound,
    OutdatedReviewTimestamp,
    InvalidContent,
}

public class SubmitReviewResult
{
    public bool IsSuccess { get; }
    public SubmitReviewFailureType? FailureType { get; }
    public string? ErrorMessage { get; }

    private SubmitReviewResult(bool isSuccess, SubmitReviewFailureType? failureType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        FailureType = failureType;
        ErrorMessage = errorMessage;
    }

    public static SubmitReviewResult Success() => new(true, null, null);

    public static SubmitReviewResult Failure(SubmitReviewFailureType failureType, string errorMessage)
        => new(false, failureType, errorMessage);
}