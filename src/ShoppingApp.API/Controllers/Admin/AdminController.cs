using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public AdminController(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private async Task<bool> IsAdmin()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return false;
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role.RoleName == "admin";
    }

    #region Dashboard Stats

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        if (!await IsAdmin()) return Forbid();

        var totalUsers = await _context.Users.CountAsync();
        var totalSellers = await _context.Shops.CountAsync(s => s.IsApproved);
        var pendingSellers = await _context.Shops.CountAsync(s => !s.IsApproved);
        var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
        var pendingReviews = await _context.Reviews.CountAsync(r => r.Status == "pending");

        return Ok(new AdminStatsDto
        {
            TotalUsers = totalUsers,
            TotalSellers = totalSellers,
            PendingSellers = pendingSellers,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingReviews = pendingReviews
        });
    }

    #endregion

    #region User Management

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        [FromQuery] string? search = null)
    {
        if (!await IsAdmin()) return Forbid();

        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Shop)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role.RoleName.ToLower() == role.ToLower());
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search) ||
                u.Email.Contains(search));
        }

        query = query.OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = users.Select(u => new AdminUserDto
        {
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role.RoleName,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Shop = u.Shop != null ? new ShopBasicDto
            {
                ShopId = u.Shop.ShopId,
                ShopName = u.Shop.ShopName,
                IsApproved = u.Shop.IsApproved
            } : null
        }).ToList();

        return Ok(new PagedResult<AdminUserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPut("users/{id:int}/suspend")]
    public async Task<ActionResult> SuspendUser(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "User suspended successfully" });
    }

    [HttpPut("users/{id:int}/activate")]
    public async Task<ActionResult> ActivateUser(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "User activated successfully" });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var user = await _context.Users
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Check if user has orders
        if (user.Orders.Any())
        {
            return BadRequest(new { message = "Cannot delete user with existing orders. Suspend instead." });
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "User deleted successfully" });
    }

    #endregion

    #region Seller Management

    [HttpGet("sellers/pending")]
    public async Task<ActionResult<List<PendingSellerDto>>> GetPendingSellers()
    {
        if (!await IsAdmin()) return Forbid();

        var pendingShops = await _context.Shops
            .Where(s => !s.IsApproved)
            .Include(s => s.Seller)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        var result = pendingShops.Select(s => new PendingSellerDto
        {
            ShopId = s.ShopId,
            ShopName = s.ShopName,
            Description = s.Description,
            LogoImageUrl = s.LogoImageUrl,
            CreatedAt = s.CreatedAt,
            Seller = new SellerInfoDto
            {
                UserId = s.Seller.UserId,
                FirstName = s.Seller.FirstName,
                LastName = s.Seller.LastName,
                Email = s.Seller.Email,
                Phone = s.Seller.Phone
            }
        }).ToList();

        return Ok(result);
    }

    [HttpPut("sellers/{id:int}/approve")]
    public async Task<ActionResult> ApproveSeller(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var shop = await _unitOfWork.Shops.GetByIdAsync(id);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        shop.IsApproved = true;
        shop.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Shops.Update(shop);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Seller approved successfully" });
    }

    [HttpPut("sellers/{id:int}/reject")]
    public async Task<ActionResult> RejectSeller(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var shop = await _context.Shops
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.ShopId == id);

        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        // Revert user role to customer
        var customerRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.RoleName == "customer");
        if (customerRole != null)
        {
            shop.Seller.RoleId = customerRole.RoleId;
            _unitOfWork.Users.Update(shop.Seller);
        }

        // Delete the shop
        _unitOfWork.Shops.Remove(shop);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Seller application rejected" });
    }

    #endregion

    #region Category Management

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (!await IsAdmin()) return Forbid();

        // Verify parent category if provided
        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.CategoryId == request.ParentCategoryId);
            if (!parentExists)
            {
                return BadRequest(new { message = "Parent category not found" });
            }
        }

        var category = new Category
        {
            CategoryName = request.CategoryName,
            ParentCategoryId = request.ParentCategoryId
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction("GetCategories", "Categories", new { id = category.CategoryId }, new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ParentCategoryId = category.ParentCategoryId
        });
    }

    [HttpPut("categories/{id:int}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        if (!await IsAdmin()) return Forbid();

        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        // Verify parent category if provided
        if (request.ParentCategoryId.HasValue)
        {
            if (request.ParentCategoryId == id)
            {
                return BadRequest(new { message = "Category cannot be its own parent" });
            }

            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.CategoryId == request.ParentCategoryId);
            if (!parentExists)
            {
                return BadRequest(new { message = "Parent category not found" });
            }
        }

        category.CategoryName = request.CategoryName;
        category.ParentCategoryId = request.ParentCategoryId;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ParentCategoryId = category.ParentCategoryId
        });
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var category = await _context.Categories
            .Include(c => c.Products)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        if (category.Products.Any())
        {
            return BadRequest(new { message = "Cannot delete category with products. Move products first." });
        }

        if (category.SubCategories.Any())
        {
            return BadRequest(new { message = "Cannot delete category with subcategories. Delete subcategories first." });
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Category deleted successfully" });
    }

    #endregion

    #region Review Moderation

    [HttpGet("reviews/pending")]
    public async Task<ActionResult<List<PendingReviewDto>>> GetPendingReviews()
    {
        if (!await IsAdmin()) return Forbid();

        var pendingReviews = await _context.Reviews
            .Where(r => r.Status == "pending")
            .Include(r => r.Product)
            .Include(r => r.User)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        var result = pendingReviews.Select(r => new PendingReviewDto
        {
            ReviewId = r.ReviewId,
            ProductId = r.ProductId,
            ProductName = r.Product.ProductName,
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            Reviewer = new ReviewerDto
            {
                UserId = r.User.UserId,
                FirstName = r.User.FirstName,
                LastName = r.User.LastName,
                Email = r.User.Email
            }
        }).ToList();

        return Ok(result);
    }

    [HttpPut("reviews/{id:int}/approve")]
    public async Task<ActionResult> ApproveReview(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var review = await _context.Reviews
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (review == null)
        {
            return NotFound(new { message = "Review not found" });
        }

        review.Status = "approved";

        // Update product average rating
        var productReviews = await _context.Reviews
            .Where(r => r.ProductId == review.ProductId && r.Status == "approved")
            .ToListAsync();

        var totalRating = productReviews.Sum(r => r.Rating) + review.Rating;
        var totalReviews = productReviews.Count + 1;

        review.Product.AverageRating = Math.Round((decimal)totalRating / totalReviews, 2);
        review.Product.TotalReviews = totalReviews;

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Review approved successfully" });
    }

    [HttpPut("reviews/{id:int}/reject")]
    public async Task<ActionResult> RejectReview(int id)
    {
        if (!await IsAdmin()) return Forbid();

        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound(new { message = "Review not found" });
        }

        review.Status = "rejected";
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Review rejected successfully" });
    }

    #endregion
}
