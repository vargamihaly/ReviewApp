using Microsoft.AspNetCore.Mvc;
using ReviewApp.Api.Dto.Request;
using ReviewApp.Application.Interfaces;
using ReviewApp.Application.Results;
using System.ComponentModel.DataAnnotations;

namespace ReviewApp.Api.Controllers;

[Route("api/[controller]/{productName}")]
[ApiController]
public class ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves the most recent reviews for a product.
    /// </summary>
    /// <param name="productName">The product to fetch reviews for.</param>
    /// <param name="limit">Maximum number of reviews to return.</param>
    [HttpGet]
    public async Task<IActionResult> GetLatestReviews([Required] string productName, [FromQuery, Range(1, 100)] int limit = 10)
    {
        try
        {
            var reviews = await reviewService.GetLatestReviewsAsync(productName, limit);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching reviews for {ProductName}", productName);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Submits a new review for a product.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    [HttpPost]
    public async Task<IActionResult> SubmitReview([Required] string productName, [FromBody, Required] ReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 500)
        {
            return BadRequest(SubmitReviewResult.Failure(SubmitReviewFailureType.InvalidContent, "Review content must be between 1 and 500 characters"));
        }

        try
        {
            var result = await reviewService.SubmitReviewAsync(
                productName,
                request.Content,
                request.LatestFetchedReviewTimestampUtc!.Value);

            return result.IsSuccess switch
            {
                true => Ok(),
                false when result.FailureType == SubmitReviewFailureType.ProductNotFound
                    => NotFound(result.ErrorMessage),
                false when result.FailureType == SubmitReviewFailureType.OutdatedReviewTimestamp
                    => Conflict(result.ErrorMessage),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error")
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting review for {ProductName}", productName);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }
}