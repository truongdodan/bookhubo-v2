using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class SalesController : Controller
    {
        private readonly BookHubDbContext _context;

        public SalesController(BookHubDbContext context)
        {
            _context = context;
        }

        // GET: /Sales - View all orders containing my books
        public async Task<IActionResult> Index(string status = "all")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<OrderItem> query = _context.OrderItems
                .Include(oi => oi.Book)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o!.Buyer)
                .Where(oi => oi.SellerId == userId.Value);

            // Filter by status
            if (status != "all")
            {
                query = query.Where(oi => oi.Status.ToLower() == status.ToLower());
            }

            var orderItems = await query.OrderByDescending(oi => oi.Order!.OrderDate).ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(orderItems);
        }

        // POST: /Sales/MarkAsShipped/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsShipped(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null || orderItem.SellerId != userId.Value)
            {
                return NotFound();
            }

            if (orderItem.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Chỉ có thể đánh dấu đơn hàng đang Pending!";
                return RedirectToAction(nameof(Index));
            }

            orderItem.Status = "Shipped";
            orderItem.ShippedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu đơn hàng là Shipped!";
            return RedirectToAction(nameof(Index));
        }
    }
}
