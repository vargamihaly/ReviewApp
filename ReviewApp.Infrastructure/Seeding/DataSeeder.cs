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

        // Reviews are defined with sequential timestamps per product
        var sampleReviews = new[]
        {
            // Smartphone reviews (oldest first)
            new { Product = "Smartphone", Content = "Good but battery could be better", Timestamp = now.AddDays(-5) },
            new { Product = "Smartphone", Content = "Battery lasts for two days!", Timestamp = now.AddDays(-3) },
            new { Product = "Smartphone", Content = "Excellent screen resolution.", Timestamp = now.AddDays(-1) },
            
            // Laptop reviews (oldest first)
            new { Product = "Laptop", Content = "Gets hot under heavy load", Timestamp = now.AddDays(-4) },
            new { Product = "Laptop", Content = "Boots up in seconds. Love it!", Timestamp = now.AddDays(-3) },
            new { Product = "Laptop", Content = "Keyboard feels premium.", Timestamp = now.AddDays(-2) },
            
            // Other products with at least one review each
            new { Product = "Headphones", Content = "Perfect noise cancellation.", Timestamp = now.AddDays(-2) },
            new { Product = "Smartwatch", Content = "Tracks my workouts accurately.", Timestamp = now.AddDays(-4) },
            new { Product = "Gaming Console", Content = "Smooth frame rates on all games.", Timestamp = now.AddDays(-3) },
            new { Product = "Bluetooth Speaker", Content = "Great bass and clarity.", Timestamp = now.AddDays(-5) },
            new { Product = "4K Monitor", Content = "Perfect for photo editing.", Timestamp = now.AddDays(-2) },
            new { Product = "Keyboard", Content = "Clicky and responsive keys.", Timestamp = now.AddDays(-3) },
            new { Product = "Mouse", Content = "Feels great in the hand.", Timestamp = now.AddDays(-1) },
            new { Product = "Router", Content = "Wi-Fi coverage is excellent.", Timestamp = now.AddDays(-1) }
        };

        // Group by product and process in chronological order
        var reviewsByProduct = sampleReviews
            .GroupBy(r => r.Product)
            .SelectMany(g => g.OrderBy(r => r.Timestamp));

        foreach (var review in reviewsByProduct)
        {
            // Get the latest existing review timestamp for this product
            var latestReviews = await reviewService.GetLatestReviewsAsync(review.Product, 1);
            var latestTimestamp = latestReviews.FirstOrDefault()?.CreatedAtUtc ?? DateTimeOffset.MinValue;

            // Submit with the correct previous timestamp
            await reviewService.SubmitReviewAsync(
                review.Product,
                review.Content,
                latestTimestamp);
        }
    }
}
