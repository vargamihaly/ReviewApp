using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Testcontainers.Azurite;

namespace ReviewApp.IntegrationTests;

internal static class Fixture
{
    public static AzuriteContainer AzureContainer { get; private set; } = null!;
    public static string ConnectionString => AzureContainer.GetConnectionString();
    public static WebApplicationFactory<Program> Factory { get; private set; } = null!;

    [Before(TestSession)]
    public static async Task SetUp()
    {
        AzureContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .Build();

        await AzureContainer.StartAsync();

        // Create table before building factory
        await CreateTableAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP3SUPPORT", "false");

                builder.ConfigureServices(services =>
                {
                    // Manually register TableServiceClient
                    services.AddSingleton(new TableServiceClient(ConnectionString));

                    // Add health checks
                    services.AddHealthChecks();
                });
            });
    }


    private static async Task CreateTableAsync()
    {
        var tableService = new TableServiceClient(ConnectionString);
        var tableClient = tableService.GetTableClient("Reviews");
        await tableClient.CreateIfNotExistsAsync();
    }

    [After(TestSession)]
    public static async Task TearDown()
    {
        await AzureContainer.DisposeAsync();
        if (Factory != null) await Factory.DisposeAsync();
    }

    public static async Task ClearTableAsync()
    {
        var tableService = new TableServiceClient(ConnectionString);
        var tableClient = tableService.GetTableClient("Reviews");

        // Ensure table exists before clearing
        await tableClient.CreateIfNotExistsAsync();

        // Efficient table clearing without recreation
        await foreach (var entity in tableClient.QueryAsync<TableEntity>())
        {
            await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
        }
    }

    public static async Task AddTestReviewAsync(
        string productName,
        string content,
        DateTimeOffset timestamp)
    {
        var tableService = new TableServiceClient(ConnectionString);
        var tableClient = tableService.GetTableClient("Reviews");

        // Ensure table exists before adding
        await tableClient.CreateIfNotExistsAsync();

        var rowKey = (DateTimeOffset.MaxValue.Ticks - timestamp.Ticks).ToString("D19");
        var entity = new TableEntity(productName, rowKey)
        {
            { "Content", content },
            { "CreatedAtUtc", timestamp },
        };

        await tableClient.AddEntityAsync(entity);
    }

    public static async Task AddProductAsync(string productName, string description)
    {
        var tableService = new TableServiceClient(ConnectionString);
        var tableClient = tableService.GetTableClient("Products");
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity(productName, "METADATA")
        {
            { "Description", description },
            { "CreatedAtUtc", DateTimeOffset.UtcNow },
        };

        await tableClient.AddEntityAsync(entity);
    }
}