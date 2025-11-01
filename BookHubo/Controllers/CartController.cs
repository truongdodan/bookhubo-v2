using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class CartController : Controller
    {
        private readonly BookHubDbContext _context;

        public CartController(BookHubDbContext context)
        {
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                    .ThenInclude(b => b.Seller)
                .Where(ci => ci.UserId == userId.Value)
                .OrderBy(ci => ci.Book.SellerId)
                .ThenBy(ci => ci.AddedAt)
                .ToListAsync();

            // Group by seller
            var groupedBySeller = cartItems
                .GroupBy(ci => ci.Book.SellerId)
                .Select(g => new CartSellerGroup
                {
                    SellerId = g.Key,
                    SellerName = g.First().Book.Seller?.FullName ?? "Unknown",
                    Items = g.ToList(),
                    Subtotal = g.Sum(ci => ci.Book.Price * ci.Quantity)
                })
                .ToList();

            var viewModel = new CartViewModel
            {
                SellerGroups = groupedBySeller,
                GrandTotal = groupedBySeller.Sum(g => g.Subtotal)
            };

            return View(viewModel);
        }
    }

    public class CartViewModel
    {
        public List<CartSellerGroup> SellerGroups { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    public class CartSellerGroup
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public List<Models.CartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
    }
}
