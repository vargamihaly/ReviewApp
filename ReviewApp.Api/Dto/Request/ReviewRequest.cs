using System.ComponentModel.DataAnnotations;

namespace ReviewApp.Api.Dto.Request;

public class ReviewRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = null!;

    [Required]
    public DateTimeOffset? LatestFetchedReviewTimestampUtc { get; set; }
}
