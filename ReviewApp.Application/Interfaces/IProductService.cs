using ReviewApp.Application.Entities;

namespace ReviewApp.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductEntity>> GetProductsAsync();
    Task<ProductEntity?> GetProductAsync(string productName);
    Task<bool> ProductExistsAsync(string productName);
    Task AddProductAsync(ProductEntity product);
    Task UpdateProductAsync(string name, string description);
    Task<bool> DeleteProductAsync(string productName);
}
