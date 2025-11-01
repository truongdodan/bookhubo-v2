using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class AdminController : Controller
    {
        private readonly BookHubDbContext _context;

        public AdminController(BookHubDbContext context)
        {
            _context = context;
        }

        // Authorization check helper
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }

        // GET: /Admin/Index - Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalActiveListings = await _context.Books.CountAsync(b => b.IsActive),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice)
            };

            return View(stats);
        }

        // GET: /Admin/Users - Manage Users
        public async Task<IActionResult> Users(string? search, string? role)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var query = _context.Users.AsQueryable();

            // Search by email or name
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search));
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewBag.SearchQuery = search;
            ViewBag.RoleFilter = role;

            return View(users);
        }

        // POST: /api/Admin/BanUser/{userId}
        [HttpPost]
        [Route("api/Admin/BanUser/{userId}")]
        public async Task<IActionResult> BanUser(int userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (user.Role == "Admin")
                {
                    return Json(new { success = false, message = "Cannot ban admin users" });
                }

                user.IsBanned = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User banned successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /api/Admin/UnbanUser/{userId}
        [HttpPost]
        [Route("api/Admin/UnbanUser/{userId}")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                user.IsBanned = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User unbanned successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: /api/Admin/DeleteUser/{userId}
        [HttpDelete]
        [Route("api/Admin/DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Cascade delete will handle related data
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Admin/Listings - Manage Listings
        public async Task<IActionResult> Listings(string? search, string? category, string? status)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var query = _context.Books.Include(b => b.Seller).AsQueryable();

            // Search by title or author
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search));
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category == category);
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Active")
                {
                    query = query.Where(b => b.IsActive);
                }
                else if (status == "Deleted")
                {
                    query = query.Where(b => !b.IsActive);
                }
            }

            var books = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.SearchQuery = search;
            ViewBag.CategoryFilter = category;
            ViewBag.StatusFilter = status;

            return View(books);
        }

        // GET: /Admin/Orders - Manage Orders
        public async Task<IActionResult> Orders(string? search, string? status)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var query = _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Seller)
                .AsQueryable();

            // Search by buyer name/email or order ID
            if (!string.IsNullOrWhiteSpace(search))
            {
                if (int.TryParse(search, out int orderId))
                {
                    query = query.Where(o => o.OrderId == orderId);
                }
                else
                {
                    query = query.Where(o =>
                        o.Buyer!.FullName.Contains(search) ||
                        o.Buyer!.Email.Contains(search));
                }
            }

            // Filter by order items status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.Status == status));
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.SearchQuery = search;
            ViewBag.StatusFilter = status;

            return View(orders);
        }

        // DELETE: /api/Admin/DeleteListing/{bookId}
        [HttpDelete]
        [Route("api/Admin/DeleteListing/{bookId}")]
        public async Task<IActionResult> DeleteListing(int bookId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "Listing not found" });
                }

                // Soft delete
                book.IsActive = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Listing deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalActiveListings { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
