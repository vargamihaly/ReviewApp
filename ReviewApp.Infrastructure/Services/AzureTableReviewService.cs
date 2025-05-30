using Azure.Data.Tables;
using ReviewApp.Application.Entities;
using ReviewApp.Application.Interfaces;
using ReviewApp.Application.Results;

namespace ReviewApp.Infrastructure.Services;

public class AzureTableReviewService : IReviewService
{
    private readonly TableClient _reviewsTableClient;
    private readonly IProductService _productService;

    public AzureTableReviewService(TableServiceClient client, IProductService productService)
    {
        _productService = productService;

        _reviewsTableClient = client.GetTableClient("Reviews");
        _reviewsTableClient.CreateIfNotExists();
    }

    public async Task<IEnumerable<ReviewEntity>> GetLatestReviewsAsync(string productName, int limit = 10)
    {
        var query = _reviewsTableClient.QueryAsync<TableEntity>(
            filter: e => e.PartitionKey == productName,
            maxPerPage: limit
        );

        var reviews = new List<ReviewEntity>();

        await foreach (var page in query.AsPages())
        {
            foreach (var entity in page.Values)
            {
                reviews.Add(new ReviewEntity
                {
                    ProductName = entity.PartitionKey,
                    Content = entity.GetString("Content"),
                    CreatedAtUtc = entity.GetDateTimeOffset("CreatedAtUtc")!.Value,
                });

                if (reviews.Count >= limit) break;
            }
            if (reviews.Count >= limit) break;
        }

        return reviews.OrderByDescending(r => r.CreatedAtUtc);
    }

    public async Task<SubmitReviewResult> SubmitReviewAsync(string productName, string content, DateTimeOffset latestFetchedReviewTimestamp)
    {
        if (!await _productService.ProductExistsAsync(productName))
        {
            return SubmitReviewResult.Failure(
                SubmitReviewFailureType.ProductNotFound,
                $"Product '{productName}' does not exist");
        }

        var latest = await GetLatestReviewsAsync(productName, 1);
        var currentLatest = latest.FirstOrDefault()?.CreatedAtUtc ?? DateTimeOffset.MinValue;

        if (currentLatest > latestFetchedReviewTimestamp)
        {
            return SubmitReviewResult.Failure(
                SubmitReviewFailureType.OutdatedReviewTimestamp,
                "New reviews have been added since you last fetched");
        }

        var now = DateTimeOffset.UtcNow;
        var rowKey = (DateTimeOffset.MaxValue.Ticks - now.Ticks).ToString("D19");

        var entity = new TableEntity(productName, rowKey)
        {
            { "Content", content },
            { "CreatedAtUtc", now },
        };

        await _reviewsTableClient.AddEntityAsync(entity);
        return SubmitReviewResult.Success();
    }
}