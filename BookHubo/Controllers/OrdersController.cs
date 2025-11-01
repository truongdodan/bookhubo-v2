using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class OrdersController : Controller
    {
        private readonly BookHubDbContext _context;

        public OrdersController(BookHubDbContext context)
        {
            _context = context;
        }

        // GET: /Orders/MyOrders - for buyers
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Where(o => o.BuyerId == userId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/{orderId}
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userRole = HttpContext.Session.GetString("UserRole");

            // Admin can view any order, regular users can only view their own orders
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Seller)
                .Where(o => o.OrderId == id);

            // If not admin, restrict to buyer's orders only
            if (userRole != "Admin")
            {
                query = query.Where(o => o.BuyerId == userId.Value);
            }

            var order = await query.FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
