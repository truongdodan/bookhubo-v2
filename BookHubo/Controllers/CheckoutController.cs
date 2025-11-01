using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly BookHubDbContext _context;

        public CheckoutController(BookHubDbContext context)
        {
            _context = context;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user info for shipping address
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get cart items
            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                    .ThenInclude(b => b.Seller)
                .Where(ci => ci.UserId == userId.Value)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index", "Cart");
            }

            // Group by seller
            var groupedBySeller = cartItems
                .GroupBy(ci => ci.Book.SellerId)
                .Select(g => new CheckoutSellerGroup
                {
                    SellerId = g.Key,
                    SellerName = g.First().Book.Seller?.FullName ?? "Unknown",
                    Items = g.ToList(),
                    Subtotal = g.Sum(ci => ci.Book.Price * ci.Quantity)
                })
                .ToList();

            var viewModel = new CheckoutViewModel
            {
                User = user,
                SellerGroups = groupedBySeller,
                GrandTotal = groupedBySeller.Sum(g => g.Subtotal)
            };

            return View(viewModel);
        }

        // POST: /Checkout/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get cart items with book info
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Book)
                    .Where(ci => ci.UserId == userId.Value)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                    return RedirectToAction("Index", "Cart");
                }

                // Get user for shipping address
                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validate stock and check for seller's own books
                foreach (var item in cartItems)
                {
                    if (item.Book.SellerId == userId.Value)
                    {
                        TempData["ErrorMessage"] = "Bạn không thể mua sách của chính mình";
                        return RedirectToAction("Index", "Cart");
                    }

                    if (item.Book.StockQuantity < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Sách '{item.Book.Title}' không đủ số lượng trong kho";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                // Calculate total
                var totalPrice = cartItems.Sum(ci => ci.Book.Price * ci.Quantity);

                // Create order
                var order = new Order
                {
                    BuyerId = userId.Value,
                    TotalPrice = totalPrice,
                    ShippingAddress = user.ShippingAddress,
                    OrderDate = DateTime.UtcNow
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items and reduce stock
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        BookId = item.BookId,
                        SellerId = item.Book.SellerId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.Book.Price,
                        Status = "Pending"
                    };
                    _context.OrderItems.Add(orderItem);

                    // Reduce stock
                    item.Book.StockQuantity -= item.Quantity;
                }

                // Delete cart items
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Confirmation", new { orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: /Checkout/Confirmation/{orderId}
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Seller)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BuyerId == userId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }

    public class CheckoutViewModel
    {
        public User User { get; set; } = null!;
        public List<CheckoutSellerGroup> SellerGroups { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    public class CheckoutSellerGroup
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
    }
}
