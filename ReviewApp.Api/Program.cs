using Asp.Versioning;
using Microsoft.OpenApi.Models;
using ReviewApp.Application.Entities;
using ReviewApp.Application.Interfaces;
using ReviewApp.Infrastructure.Extensions;
using ReviewApp.Infrastructure.Seeding;
using ReviewApp.ServiceDefaults;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructureServices();

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(majorVersion: 1, minorVersion: 0);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();


builder.AddAzureTableClient("strTables");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Review API", Version = "v1" });
    c.EnableAnnotations();
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    try
    {
        await seeder.SeedDevelopmentDataAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();
