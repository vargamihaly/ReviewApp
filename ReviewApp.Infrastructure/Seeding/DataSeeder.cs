using ReviewApp.Application.Entities;
using ReviewApp.Application.Interfaces;

namespace ReviewApp.Infrastructure.Seeding;

public class DataSeeder(IProductService productService, IReviewService reviewService)
{
    public async Task SeedDevelopmentDataAsync()
    {
        await SeedSampleProducts();
        await SeedSampleReviews();
    }

    private async Task SeedSampleProducts()
    {
        var sampleProducts = new[]
        {
            new ProductEntity { Name = "Smartphone", Description = "Latest model smartphone" },
            new ProductEntity { Name = "Laptop", Description = "High-performance laptop" },
            new ProductEntity { Name = "Headphones", Description = "Noise-cancelling headphones" },
            new ProductEntity { Name = "Smartwatch", Description = "Stylish and smart wearable device" },
            new ProductEntity { Name = "Gaming Console", Description = "Next-gen gaming experience" },
            new ProductEntity { Name = "Bluetooth Speaker", Description = "Portable and powerful sound" },
            new ProductEntity { Name = "4K Monitor", Description = "Crisp and detailed display" },
            new ProductEntity { Name = "Keyboard", Description = "Mechanical backlit keyboard" },
            new ProductEntity { Name = "Mouse", Description = "Ergonomic wireless mouse" },
            new ProductEntity { Name = "Router", Description = "High-speed Wi-Fi 6 router" }
        };

        foreach (var product in sampleProducts)
        {
            if (!await productService.ProductExistsAsync(product.Name))
            {
                await productService.AddProductAsync(product);
            }
        }
    }

    private async Task SeedSampleReviews()
    {
        var now = DateTimeOffset.UtcNow;
        var sampleReviews = new[]
        {
            new { Product = "Smartphone", Content = "Battery lasts for two days!", Timestamp = now.AddDays(-2) },
            new { Product = "Smartphone", Content = "Excellent screen resolution.", Timestamp = now.AddDays(-1) },
            new { Product = "Laptop", Content = "Boots up in seconds. Love it!", Timestamp = now.AddDays(-3) },
            new { Product = "Laptop", Content = "Keyboard feels premium.", Timestamp = now.AddDays(-2) },
            new { Product = "Headphones", Content = "Perfect noise cancellation.", Timestamp = now.AddDays(-1) },
            new { Product = "Smartwatch", Content = "Tracks my workouts accurately.", Timestamp = now.AddDays(-4) },
            new { Product = "Gaming Console", Content = "Smooth frame rates on all games.", Timestamp = now.AddDays(-3) },
            new { Product = "Bluetooth Speaker", Content = "Great bass and clarity.", Timestamp = now.AddDays(-5) },
            new { Product = "4K Monitor", Content = "Perfect for photo editing.", Timestamp = now.AddDays(-2) },
            new { Product = "Keyboard", Content = "Clicky and responsive keys.", Timestamp = now.AddDays(-3) },
            new { Product = "Mouse", Content = "Feels great in the hand.", Timestamp = now.AddDays(-1) },
            new { Product = "Router", Content = "Wi-Fi coverage is excellent.", Timestamp = now.AddDays(-1) }
        };

        foreach (var review in sampleReviews)
        {
            await reviewService.SubmitReviewAsync(
                review.Product,
                review.Content,
                DateTimeOffset.MinValue);
        }
    }
}
