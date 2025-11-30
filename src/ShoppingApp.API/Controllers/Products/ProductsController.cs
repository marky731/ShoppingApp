using Microsoft.AspNetCore.Mvc;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Interfaces.Repositories;

namespace ShoppingApp.API.Controllers.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetProducts([FromQuery] ProductQueryParams query)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetProductsAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.CategoryId,
            query.MinPrice,
            query.MaxPrice,
            query.Brand,
            query.SortBy);

        var productDtos = products.Select(p => new ProductListDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Slug = p.Slug,
            Price = p.Price,
            MainImageUrl = p.MainImageUrl,
            Brand = p.Brand,
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            ShopName = p.Shop.ShopName,
            CategoryName = p.Category.CategoryName
        }).ToList();

        return Ok(new PagedResult<ProductListDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(MapToDetailDto(product));
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductBySlug(string slug)
    {
        var product = await _unitOfWork.Products.GetProductBySlugAsync(slug);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(MapToDetailDto(product));
    }

    private static ProductDetailDto MapToDetailDto(Core.Entities.Product product)
    {
        return new ProductDetailDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            MainImageUrl = product.MainImageUrl,
            Brand = product.Brand,
            Model = product.Model,
            AverageRating = product.AverageRating,
            TotalReviews = product.TotalReviews,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            Shop = new ShopSummaryDto
            {
                ShopId = product.Shop.ShopId,
                ShopName = product.Shop.ShopName,
                Description = product.Shop.Description,
                LogoImageUrl = product.Shop.LogoImageUrl,
                AverageRating = product.Shop.AverageRating,
                TotalSales = product.Shop.TotalSales
            },
            Category = new CategoryDto
            {
                CategoryId = product.Category.CategoryId,
                CategoryName = product.Category.CategoryName,
                ParentCategoryId = product.Category.ParentCategoryId
            },
            Images = product.Images.Select(i => i.ImageUrl).ToList(),
            Specifications = product.Specifications.Select(s => new ProductSpecificationDto
            {
                SpecificationId = s.SpecificationId,
                SpecName = s.SpecName,
                SpecValue = s.SpecValue,
                DisplayOrder = s.DisplayOrder
            }).ToList()
        };
    }
}
