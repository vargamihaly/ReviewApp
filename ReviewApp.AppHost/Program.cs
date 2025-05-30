using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var azStorage = builder.AddAzureStorage("azstorage");

if (builder.Environment.IsDevelopment())
{
    azStorage.RunAsEmulator();
}

var strTables = azStorage.AddTables("strTables");

builder.AddProject<Projects.ReviewApp_Api>("reviewapp-api")
    .WithExternalHttpEndpoints()
    .WithReference(strTables)
    .WaitFor(strTables);

await builder.Build().RunAsync();