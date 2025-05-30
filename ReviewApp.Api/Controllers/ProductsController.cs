using Microsoft.AspNetCore.Mvc;
using ReviewApp.Api.Dto.Request;
using ReviewApp.Application.Entities;
using ReviewApp.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ReviewApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(
    IProductService productService,
    ILogger<ProductsController> logger) : ControllerBase
{
    /// <summary>
    /// Returns a list of all products.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var products = await productService.GetProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Checks whether a product exists.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    [HttpGet("{productName}")]
    public async Task<IActionResult> GetProduct(string productName)
    {
        try
        {
            var exists = await productService.ProductExistsAsync(productName);
            return exists ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking product {ProductName}", productName);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody, Required] ProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await productService.AddProductAsync(new ProductEntity
            {
                Name = request.Name,
                Description = request.Description,
            });

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product {ProductName}", request.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Updates an existing product's description.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    [HttpPut("{productName}")]
    public async Task<IActionResult> UpdateProduct(
    string productName,
    [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var existing = await productService.GetProductAsync(productName);
            if (existing == null)
                return NotFound();

            existing.Description = request.Description;

            await productService.UpdateProductAsync(productName, request.Description);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product {ProductName}", productName);
            return StatusCode(StatusCodes.Status500InternalServerError, "Update failed");
        }
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    [HttpDelete("{productName}")]
    public async Task<IActionResult> DeleteProduct(string productName)
    {
        try
        {
            var success = await productService.DeleteProductAsync(productName);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {ProductName}", productName);
            return StatusCode(StatusCodes.Status500InternalServerError, "Deletion failed");
        }
    }
}
