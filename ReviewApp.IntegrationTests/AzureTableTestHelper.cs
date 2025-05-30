using Azure.Data.Tables;
using System.Threading.Tasks;
using System;

namespace ReviewApp.IntegrationTests;

public static class AzureTableTestHelper
{
    public static async Task ClearTableAsync(string connectionString)
    {
        var tableService = new TableServiceClient(connectionString);
        var tableClient = tableService.GetTableClient("Reviews");
        await tableClient.DeleteAsync();
        await tableClient.CreateIfNotExistsAsync();
    }

    public static async Task AddTestReviewAsync(
        string connectionString,
        string productName,
        string content,
        DateTimeOffset timestamp)
    {
        var tableService = new TableServiceClient(connectionString);
        var tableClient = tableService.GetTableClient("Reviews");

        var rowKey = (DateTimeOffset.MaxValue.Ticks - timestamp.Ticks).ToString("D19");
        var entity = new TableEntity(productName, rowKey)
        {
            { "Content", content },
            { "CreatedAtUtc", timestamp },
        };

        await tableClient.AddEntityAsync(entity);
    }
}