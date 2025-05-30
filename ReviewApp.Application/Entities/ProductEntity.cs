namespace ReviewApp.Application.Entities;

public class ProductEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    //public DateTimeOffset? ModifiedAtUtc { get; set; }
}
