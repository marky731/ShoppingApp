namespace ShoppingApp.API.Models.DTOs;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ReviewResponseDto? Response { get; set; }
}

public class ReviewResponseDto
{
    public int ResponseId { get; set; }
    public string ResponseText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string SellerName { get; set; } = string.Empty;
}

public class CreateReviewDto
{
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

public class ProductReviewsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
}
