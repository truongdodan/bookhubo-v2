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
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
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
        public async Task<IActionResult> Listings()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var books = await _context.Books
                .Include(b => b.Seller)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(books);
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
