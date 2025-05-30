using ReviewApp.Application.Entities;
using ReviewApp.Application.Results;

namespace ReviewApp.Application.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<ReviewEntity>> GetLatestReviewsAsync(string productName, int limit = 10);
    Task<SubmitReviewResult> SubmitReviewAsync(string productName, string content, DateTimeOffset latestFetchedReviewTimestamp);
}
