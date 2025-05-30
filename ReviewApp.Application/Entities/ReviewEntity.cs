namespace ReviewApp.Application.Entities;

public class ReviewEntity
{
    public string ProductName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
}