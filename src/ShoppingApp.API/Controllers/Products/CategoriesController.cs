using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Products;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .Include(c => c.SubCategories)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                ParentCategoryId = c.ParentCategoryId,
                SubCategories = c.SubCategories.Select(sc => new CategoryDto
                {
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.CategoryName,
                    ParentCategoryId = sc.ParentCategoryId
                }).ToList()
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                ParentCategoryId = c.ParentCategoryId,
                SubCategories = c.SubCategories.Select(sc => new CategoryDto
                {
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.CategoryName,
                    ParentCategoryId = sc.ParentCategoryId
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }
}
