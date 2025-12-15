using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Seller.Controllers
{
    [Authorize(Roles = "seller")]
    public class ReviewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Seller/Reviews
        public ActionResult Index(int? rating, int page = 1)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var query = db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Include(r => r.Responses)
                .Where(r => r.Product.ShopId == shopId && r.Status == ReviewStatus.Approved);

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            var reviews = query.OrderByDescending(r => r.CreatedAt).ToList();

            var viewModel = reviews.Select(r => new SellerReviewListItemViewModel
            {
                ReviewId = r.ReviewId,
                ProductId = r.ProductId,
                ProductName = r.Product.ProductName,
                ProductImageUrl = r.Product.MainImageUrl,
                CustomerName = r.User.FirstName + " " + r.User.LastName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                HasResponse = r.Response != null
            });

            ViewBag.RatingFilter = rating;
            ViewBag.ShopName = shop.ShopName;

            // Calculate statistics
            var allReviews = db.Reviews
                .Where(r => r.Product.ShopId == shopId && r.Status == ReviewStatus.Approved)
                .ToList();

            ViewBag.TotalReviews = allReviews.Count;
            ViewBag.AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;
            ViewBag.ResponseRate = allReviews.Any()
                ? (double)allReviews.Count(r => db.ReviewResponses.Any(rr => rr.ReviewId == r.ReviewId)) / allReviews.Count * 100
                : 0;

            return View(viewModel.ToPagedList(page, 10));
        }

        // GET: Seller/Reviews/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var review = db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Include(r => r.Order)
                .Include(r => r.Responses)
                .FirstOrDefault(r => r.ReviewId == id && r.Product.ShopId == shopId);

            if (review == null)
            {
                return HttpNotFound();
            }

            var viewModel = new SellerReviewDetailsViewModel
            {
                Review = review,
                Product = review.Product,
                Customer = review.User,
                Response = review.Response
            };

            return View(viewModel);
        }

        // POST: Seller/Reviews/Respond/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Respond(int id, string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                TempData["Error"] = "Response text is required.";
                return RedirectToAction("Details", new { id });
            }

            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var review = db.Reviews
                .Include(r => r.Product)
                .FirstOrDefault(r => r.ReviewId == id && r.Product.ShopId == shopId);

            if (review == null)
            {
                return HttpNotFound();
            }

            // Check if response already exists
            var existingResponse = db.ReviewResponses.FirstOrDefault(rr => rr.ReviewId == id);
            if (existingResponse != null)
            {
                // Update existing response
                existingResponse.ResponseText = responseText;
                existingResponse.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new response
                var response = new ReviewResponse
                {
                    ReviewId = id,
                    SellerId = userId,
                    ResponseText = responseText,
                    CreatedAt = DateTime.UtcNow
                };
                db.ReviewResponses.Add(response);
            }

            db.SaveChanges();

            TempData["Success"] = "Response submitted successfully.";
            return RedirectToAction("Details", new { id });
        }

        // POST: Seller/Reviews/DeleteResponse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteResponse(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var response = db.ReviewResponses
                .Include(rr => rr.Review.Product)
                .FirstOrDefault(rr => rr.ResponseId == id && rr.SellerId == userId);

            if (response == null)
            {
                return HttpNotFound();
            }

            var reviewId = response.ReviewId;
            db.ReviewResponses.Remove(response);
            db.SaveChanges();

            TempData["Success"] = "Response deleted.";
            return RedirectToAction("Details", new { id = reviewId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
