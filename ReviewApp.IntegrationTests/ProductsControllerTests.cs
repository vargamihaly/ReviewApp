using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ReviewApp.Application.Entities;

namespace ReviewApp.IntegrationTests;

public class ProductControllerTests
{
    [Before(Test)]
    public async Task ClearTableAndSeedProduct()
    {
        await Fixture.ClearTableAsync();
        await Fixture.AddProductAsync("product1", "Test Product");
    }

    [Test]
    public async Task GetProducts_ReturnsSeededProducts()
    {
        // Arrange
        var client = Fixture.Factory.CreateClient();

        // Add test products
        await Fixture.AddProductAsync("product2", "Second Product");
        await Fixture.AddProductAsync("product3", "Third Product");

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductEntity>>();
        products.Should().HaveCount(3);
    }
}