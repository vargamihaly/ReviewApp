using System.ComponentModel.DataAnnotations;

namespace ReviewApp.Api.Dto.Request;

public class ProductRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;
}