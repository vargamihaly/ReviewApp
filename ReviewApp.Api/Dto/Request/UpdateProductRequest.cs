using System.ComponentModel.DataAnnotations;

namespace ReviewApp.Api.Dto.Request
{
    public class UpdateProductRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Description { get; set; } = null!;
    }
}
