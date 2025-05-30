using Azure;
using Azure.Data.Tables;
using ReviewApp.Application.Entities;
using ReviewApp.Application.Interfaces;

namespace ReviewApp.Infrastructure.Services;

public class AzureTableProductService : IProductService
{
    private readonly TableClient productsTableClient;

    public AzureTableProductService(TableServiceClient client)
    {
        productsTableClient = client.GetTableClient("Products");
        productsTableClient.CreateIfNotExists();
    }

    public async Task<bool> ProductExistsAsync(string productName)
    {
        try
        {
            await productsTableClient.GetEntityAsync<TableEntity>(productName, "METADATA");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<IEnumerable<ProductEntity>> GetProductsAsync()
    {
        var products = new List<ProductEntity>();

        await foreach (var entity in productsTableClient.QueryAsync<TableEntity>(
                           filter: "RowKey eq 'METADATA'"))
        {
            products.Add(new ProductEntity
            {
                Name = entity.PartitionKey,
                Description = entity.GetString("Description"),
                CreatedAtUtc = entity.GetDateTimeOffset("CreatedAtUtc")!.Value,
            });
        }

        return products;
    }

    public async Task<ProductEntity?> GetProductAsync(string productName)
    {
        try
        {
            var entityResponse = await productsTableClient.GetEntityAsync<TableEntity>(productName, "METADATA");
            var entity = entityResponse.Value;

            return new ProductEntity
            {
                Name = entity.PartitionKey,
                Description = entity.GetString("Description"),
                CreatedAtUtc = entity.GetDateTimeOffset("CreatedAtUtc")!.Value,
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task AddProductAsync(ProductEntity product)
    {
        var entity = new TableEntity(product.Name, "METADATA")
        {
            { "Description", product.Description },
            { "CreatedAtUtc", product.CreatedAtUtc },
        };

        await productsTableClient.AddEntityAsync(entity);
    }

    public async Task UpdateProductAsync(string name, string description)
    {
        var existing = await productsTableClient.GetEntityAsync<TableEntity>(name, "METADATA");
        var entity = existing.Value;

        entity["Description"] = description;

        await productsTableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
    }

    public async Task<bool> DeleteProductAsync(string productName)
    {
        try
        {
            await productsTableClient.DeleteEntityAsync(productName, "METADATA");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
