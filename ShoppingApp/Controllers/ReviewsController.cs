using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Reviews/Create
        public ActionResult Create(int productId, int? orderId = null)
        {
            var userId = User.Identity.GetUserId();

            // If orderId not provided, find a delivered order with this product
            if (!orderId.HasValue)
            {
                var deliveredOrder = db.ShopOrders
                    .Where(so => so.Order.UserId == userId
                        && so.ShopOrderStatus == OrderStatus.Delivered
                        && so.OrderItems.Any(oi => oi.ProductId == productId))
                    .Select(so => so.Order.OrderId)
                    .FirstOrDefault();

                if (deliveredOrder == 0)
                {
                    TempData["Error"] = "You can only review products from delivered orders.";
                    return RedirectToAction("Details", "Products", new { id = productId });
                }

                orderId = deliveredOrder;
            }

            // Verify user ordered this product and it was delivered
            var canReview = db.ShopOrders
                .Any(so => so.Order.UserId == userId
                    && so.Order.OrderId == orderId.Value
                    && so.ShopOrderStatus == OrderStatus.Delivered
                    && so.OrderItems.Any(oi => oi.ProductId == productId));

            if (!canReview)
            {
                TempData["Error"] = "You can only review products from delivered orders.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Check if already reviewed
            var existingReview = db.Reviews.FirstOrDefault(r =>
                r.UserId == userId && r.ProductId == productId && r.OrderId == orderId.Value);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this product for this order.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Product = product;
            ViewBag.OrderId = orderId.Value;

            return View(new Review { ProductId = productId, OrderId = orderId.Value });
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Review model)
        {
            var userId = User.Identity.GetUserId();

            // Verify user ordered this product and it was delivered
            var canReview = db.ShopOrders
                .Any(so => so.Order.UserId == userId
                    && so.Order.OrderId == model.OrderId
                    && so.ShopOrderStatus == OrderStatus.Delivered
                    && so.OrderItems.Any(oi => oi.ProductId == model.ProductId));

            if (!canReview)
            {
                TempData["Error"] = "You can only review products from delivered orders.";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            // Check if already reviewed
            var existingReview = db.Reviews.FirstOrDefault(r =>
                r.UserId == userId && r.ProductId == model.ProductId && r.OrderId == model.OrderId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this product for this order.";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    OrderId = model.OrderId,
                    Rating = model.Rating,
                    Title = model.Title,
                    Comment = model.Comment,
                    Status = ReviewStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                db.Reviews.Add(review);
                db.SaveChanges();

                TempData["Success"] = "Thank you for your review! It will be visible after moderation.";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            var product = db.Products.FirstOrDefault(p => p.ProductId == model.ProductId);
            ViewBag.Product = product;
            ViewBag.OrderId = model.OrderId;

            return View(model);
        }

        // GET: Reviews/Edit/5
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var review = db.Reviews
                .Include(r => r.Product)
                .FirstOrDefault(r => r.ReviewId == id && r.UserId == userId);

            if (review == null)
            {
                return HttpNotFound();
            }

            // Can only edit pending reviews
            if (review.Status != ReviewStatus.Pending)
            {
                TempData["Error"] = "You can only edit pending reviews.";
                return RedirectToAction("Details", "Products", new { id = review.ProductId });
            }

            ViewBag.Product = review.Product;
            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Review model)
        {
            var userId = User.Identity.GetUserId();
            var review = db.Reviews.FirstOrDefault(r => r.ReviewId == id && r.UserId == userId);

            if (review == null)
            {
                return HttpNotFound();
            }

            if (review.Status != ReviewStatus.Pending)
            {
                TempData["Error"] = "You can only edit pending reviews.";
                return RedirectToAction("Details", "Products", new { id = review.ProductId });
            }

            if (ModelState.IsValid)
            {
                review.Rating = model.Rating;
                review.Title = model.Title;
                review.Comment = model.Comment;

                db.SaveChanges();

                TempData["Success"] = "Review updated successfully.";
                return RedirectToAction("Details", "Products", new { id = review.ProductId });
            }

            var product = db.Products.FirstOrDefault(p => p.ProductId == review.ProductId);
            ViewBag.Product = product;
            return View(model);
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var review = db.Reviews.FirstOrDefault(r => r.ReviewId == id && r.UserId == userId);

            if (review == null)
            {
                return HttpNotFound();
            }

            var productId = review.ProductId;

            // Delete response if exists
            var response = db.ReviewResponses.FirstOrDefault(rr => rr.ReviewId == id);
            if (response != null)
            {
                db.ReviewResponses.Remove(response);
            }

            db.Reviews.Remove(review);
            db.SaveChanges();

            // Update product average rating
            UpdateProductRating(productId);

            TempData["Success"] = "Review deleted successfully.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        private void UpdateProductRating(int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                var approvedReviews = db.Reviews
                    .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
                    .ToList();

                if (approvedReviews.Any())
                {
                    product.AverageRating = (decimal)approvedReviews.Average(r => r.Rating);
                    product.TotalReviews = approvedReviews.Count;
                }
                else
                {
                    product.AverageRating = 0;
                    product.TotalReviews = 0;
                }

                db.SaveChanges();
            }
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
