using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Infrastructure.Data;
using System.Security.Claims;

namespace ShoppingApp.API.Controllers.Reviews;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    // GET: api/reviews/product/{productId}
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ProductReviewsDto>> GetProductReviews(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Response)
                .ThenInclude(resp => resp!.Seller)
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var ratingDistribution = reviews
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        // Ensure all ratings 1-5 are in the distribution
        for (int i = 1; i <= 5; i++)
        {
            if (!ratingDistribution.ContainsKey(i))
                ratingDistribution[i] = 0;
        }

        return new ProductReviewsDto
        {
            ProductId = productId,
            ProductName = product.ProductName,
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            TotalReviews = reviews.Count,
            RatingDistribution = ratingDistribution,
            Reviews = reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                ProductId = r.ProductId,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UserName = $"{r.User.FirstName} {r.User.LastName?[0]}.",
                Response = r.Response != null ? new ReviewResponseDto
                {
                    ResponseId = r.Response.ResponseId,
                    ResponseText = r.Response.ResponseText,
                    CreatedAt = r.Response.CreatedAt,
                    SellerName = r.Response.Seller.FirstName
                } : null
            }).ToList()
        };
    }

    // POST: api/reviews
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto dto)
    {
        var userId = GetUserId();

        // Verify the user has purchased this product in the specified order
        // OrderItems -> ShopOrder -> Order
        var orderItem = await _context.OrderItems
            .Include(oi => oi.ShopOrder)
                .ThenInclude(so => so.Order)
            .FirstOrDefaultAsync(oi =>
                oi.ShopOrder.Order.OrderId == dto.OrderId &&
                oi.ShopOrder.Order.UserId == userId &&
                oi.ProductId == dto.ProductId &&
                oi.ShopOrder.ShopOrderStatus == "delivered");

        if (orderItem == null)
            return BadRequest(new { message = "You can only review products from delivered orders" });

        // Check if user already reviewed this product for this order
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r =>
                r.ProductId == dto.ProductId &&
                r.UserId == userId &&
                r.OrderId == dto.OrderId);

        if (existingReview != null)
            return BadRequest(new { message = "You have already reviewed this product for this order" });

        // Validate rating
        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest(new { message = "Rating must be between 1 and 5" });

        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment,
            Status = "approved", // Auto-approve for now, could be "pending" for moderation
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update product average rating
        await UpdateProductRating(dto.ProductId);

        var user = await _context.Users.FindAsync(userId);
        return CreatedAtAction(nameof(GetProductReviews), new { productId = dto.ProductId }, new ReviewDto
        {
            ReviewId = review.ReviewId,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            Status = review.Status,
            CreatedAt = review.CreatedAt,
            UserName = $"{user!.FirstName} {user.LastName?[0]}."
        });
    }

    // GET: api/reviews/my-reviews
    [Authorize]
    [HttpGet("my-reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetMyReviews()
    {
        var userId = GetUserId();

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Include(r => r.Response)
                .ThenInclude(resp => resp!.Seller)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new ReviewDto
        {
            ReviewId = r.ReviewId,
            ProductId = r.ProductId,
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            UserName = $"{r.User.FirstName} {r.User.LastName?[0]}.",
            Response = r.Response != null ? new ReviewResponseDto
            {
                ResponseId = r.Response.ResponseId,
                ResponseText = r.Response.ResponseText,
                CreatedAt = r.Response.CreatedAt,
                SellerName = r.Response.Seller.FirstName
            } : null
        }).ToList();
    }

    // PUT: api/reviews/{id}
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto dto)
    {
        var userId = GetUserId();

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == id && r.UserId == userId);
        if (review == null)
            return NotFound(new { message = "Review not found" });

        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest(new { message = "Rating must be between 1 and 5" });

        review.Rating = dto.Rating;
        review.Title = dto.Title;
        review.Comment = dto.Comment;

        await _context.SaveChangesAsync();
        await UpdateProductRating(review.ProductId);

        return NoContent();
    }

    // DELETE: api/reviews/{id}
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = GetUserId();

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == id && r.UserId == userId);
        if (review == null)
            return NotFound(new { message = "Review not found" });

        var productId = review.ProductId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        await UpdateProductRating(productId);

        return NoContent();
    }

    // GET: api/reviews/can-review/{productId}
    [Authorize]
    [HttpGet("can-review/{productId}")]
    public async Task<ActionResult> CanReviewProduct(int productId)
    {
        var userId = GetUserId();

        // Find orders with this product that are delivered and not yet reviewed
        // OrderItems -> ShopOrder -> Order
        var eligibleOrders = await _context.OrderItems
            .Include(oi => oi.ShopOrder)
                .ThenInclude(so => so.Order)
            .Where(oi =>
                oi.ProductId == productId &&
                oi.ShopOrder.Order.UserId == userId &&
                oi.ShopOrder.ShopOrderStatus == "delivered")
            .Select(oi => oi.ShopOrder.Order.OrderId)
            .Distinct()
            .ToListAsync();

        // Get orders that already have reviews
        var reviewedOrders = await _context.Reviews
            .Where(r => r.ProductId == productId && r.UserId == userId)
            .Select(r => r.OrderId)
            .ToListAsync();

        var canReviewOrders = eligibleOrders.Except(reviewedOrders).ToList();

        return Ok(new
        {
            canReview = canReviewOrders.Any(),
            eligibleOrderIds = canReviewOrders
        });
    }

    private async Task UpdateProductRating(int productId)
    {
        var avgRating = await _context.Reviews
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.AverageRating = (decimal)avgRating;
            await _context.SaveChangesAsync();
        }
    }
}
