using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class ReviewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Reviews/Pending
        public ActionResult Pending(int page = 1)
        {
            var pendingReviews = db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.Status == ReviewStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToPagedList(page, 10);

            var viewModel = new PendingReviewsViewModel
            {
                PendingReviews = pendingReviews
            };

            return View(viewModel);
        }

        // GET: Admin/Reviews/Details/5
        public ActionResult Details(int id)
        {
            var review = db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Include(r => r.Order)
                .FirstOrDefault(r => r.ReviewId == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ReviewModerationViewModel
            {
                Review = review,
                Product = review.Product,
                User = review.User
            };

            return View(viewModel);
        }

        // POST: Admin/Reviews/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var review = db.Reviews.FirstOrDefault(r => r.ReviewId == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            review.Status = ReviewStatus.Approved;
            db.SaveChanges();

            // Update product average rating
            UpdateProductRating(review.ProductId);

            TempData["Success"] = "Review approved successfully.";
            return RedirectToAction("Pending");
        }

        // POST: Admin/Reviews/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            var review = db.Reviews.FirstOrDefault(r => r.ReviewId == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            review.Status = ReviewStatus.Rejected;
            db.SaveChanges();

            TempData["Success"] = "Review rejected.";
            return RedirectToAction("Pending");
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
