using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin/Users
        public ActionResult Index(string search, string role, string status, int page = 1)
        {
            var query = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                query = query.Where(u => u.IsActive == isActive);
            }

            // Get all users and their roles
            var users = query.OrderByDescending(u => u.CreatedAt).ToList();

            // Map to ViewModels with roles
            var userViewModels = users.Select(u => new AdminUserListItemViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Roles = UserManager.GetRoles(u.Id),
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
            });

            // Filter by role if specified
            if (!string.IsNullOrEmpty(role))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(role));
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;

            return View(userViewModels.ToPagedList(page, 20));
        }

        // GET: Admin/Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = await UserManager.GetRolesAsync(id);

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                Roles = roles,
                OrderCount = db.Orders.Count(o => o.UserId == id),
                TotalSpent = db.Orders.Where(o => o.UserId == id).Sum(o => (decimal?)o.TotalAmount) ?? 0
            };

            return View(viewModel);
        }

        // POST: Admin/Users/Suspend/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Suspend(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Don't allow suspending yourself
            if (id == User.Identity.GetUserId())
            {
                TempData["Error"] = "You cannot suspend yourself.";
                return RedirectToAction("Index");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await UserManager.UpdateAsync(user);

            TempData["Success"] = "User suspended successfully.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Users/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await UserManager.UpdateAsync(user);

            TempData["Success"] = "User activated successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
