using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly BookHubDbContext _context;

        public ReviewsController(BookHubDbContext context)
        {
            _context = context;
        }

        // GET: /Reviews/Create/{orderItemId}
        public async Task<IActionResult> Create(int orderItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get order item with related data
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Book)
                .Include(oi => oi.Order)
                .Include(oi => oi.Seller)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);

            if (orderItem == null)
            {
                return NotFound();
            }

            // Check if current user is the buyer
            if (orderItem.Order?.BuyerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền đánh giá đơn hàng này";
                return RedirectToAction("MyOrders", "Orders");
            }

            // Check if order item is completed
            if (orderItem.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Chỉ có thể đánh giá đơn hàng đã hoàn thành";
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            // Check if review already exists
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.OrderItemId == orderItemId);

            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá đơn hàng này rồi";
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            ViewBag.OrderItem = orderItem;
            return View();
        }

        // POST: /Reviews/Create/{orderItemId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderItemId, int rating, string? comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get order item
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);

            if (orderItem == null)
            {
                return NotFound();
            }

            // Validate
            if (orderItem.Order?.BuyerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền đánh giá đơn hàng này";
                return RedirectToAction("MyOrders", "Orders");
            }

            if (orderItem.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Chỉ có thể đánh giá đơn hàng đã hoàn thành";
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Đánh giá phải từ 1 đến 5 sao";
                return RedirectToAction("Create", new { orderItemId });
            }

            // Check if review already exists
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.OrderItemId == orderItemId);

            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá đơn hàng này rồi";
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            // Create review
            var review = new Review
            {
                OrderItemId = orderItemId,
                BuyerId = userId.Value,
                SellerId = orderItem.SellerId,
                BookId = orderItem.BookId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update book's rating
            await UpdateBookRating(orderItem.BookId);

            TempData["SuccessMessage"] = "Đã gửi đánh giá thành công!";
            return RedirectToAction("Details", "Books", new { id = orderItem.BookId });
        }

        private async Task UpdateBookRating(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return;

            // Calculate average rating for this book
            var reviews = await _context.Reviews
                .Where(r => r.BookId == bookId)
                .ToListAsync();

            book.TotalReviews = reviews.Count;
            book.AverageRating = reviews.Count > 0
                ? (decimal)reviews.Average(r => r.Rating)
                : 0;

            await _context.SaveChangesAsync();
        }
    }
}
