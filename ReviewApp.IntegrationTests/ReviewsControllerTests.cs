using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using ReviewApp.Api.Dto.Request;
using ReviewApp.Application.Entities;

namespace ReviewApp.IntegrationTests;

public class ReviewsControllerTests
{
    [Before(Test)]
    public async Task ClearTable()
    {
        await Fixture.ClearTableAsync();
    }

    [Test]
    public async Task GetLatestReviews_NoReviews_ReturnsEmptyList()
    {
        var client = Fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/reviews/product1?limit=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewEntity>>();
        reviews.Should().BeEmpty();
    }

    [Test]
    public async Task GetLatestReviews_WithReviews_ReturnsOrderedResults()
    {
        var client = Fixture.Factory.CreateClient();
        var now = DateTimeOffset.UtcNow;

        await Fixture.AddTestReviewAsync("product1", "Review 1", now.AddMinutes(-10));
        await Fixture.AddTestReviewAsync("product1", "Review 2", now.AddMinutes(-5));
        await Fixture.AddTestReviewAsync("product1", "Review 3", now);

        var response = await client.GetAsync("/api/reviews/product1?limit=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewEntity>>();
        reviews.Should().HaveCount(2);
        reviews[0].Content.Should().Be("Review 3");
        reviews[1].Content.Should().Be("Review 2");
    }

    [Test]
    public async Task SubmitReview_ValidRequest_ReturnsOk()
    {
        var client = Fixture.Factory.CreateClient();
        var request = new ReviewRequest
        {
            Content = "Great product!",
            LatestFetchedReviewTimestampUtc = DateTimeOffset.MinValue,
        };

        var response = await client.PostAsJsonAsync("/api/reviews/product1", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify storage
        var tableService = new TableServiceClient(Fixture.ConnectionString);
        var tableClient = tableService.GetTableClient("Reviews");
        var entities = new List<TableEntity>();

        await foreach (var entity in tableClient.QueryAsync<TableEntity>())
        {
            entities.Add(entity);
        }

        entities.Should().HaveCount(1);
        entities[0].GetString("Content").Should().Be("Great product!");
    }

    [Test]
    public async Task SubmitReview_WithoutFetchingLatest_ReturnsConflictWithMessage()
    {
        // Arrange
        var client = Fixture.Factory.CreateClient();
        var existingReviewTime = DateTimeOffset.UtcNow;

        // Ensure product exists
        await Fixture.AddProductAsync("product1", "Test Product");
        await Fixture.AddTestReviewAsync("product1", "Existing review", existingReviewTime);

        var request = new ReviewRequest
        {
            Content = "New review",
            LatestFetchedReviewTimestampUtc = existingReviewTime.AddMinutes(-1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/reviews/product1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("refresh");
    }

    [Test]
    public async Task SubmitReview_ForNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var client = Fixture.Factory.CreateClient();
        var request = new ReviewRequest
        {
            Content = "Invalid review",
            LatestFetchedReviewTimestampUtc = DateTimeOffset.MinValue
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/reviews/non-existing-product", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("does not exist");
    }
}