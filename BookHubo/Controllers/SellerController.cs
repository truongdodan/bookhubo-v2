using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class SellerController : Controller
    {
        private readonly BookHubDbContext _context;

        public SellerController(BookHubDbContext context)
        {
            _context = context;
        }

        // GET: /Seller/PublicProfile/{sellerId}
        public async Task<IActionResult> PublicProfile(int id)
        {
            var seller = await _context.Users.FindAsync(id);
            if (seller == null)
            {
                return NotFound();
            }

            // Get active books by this seller
            var books = await _context.Books
                .Where(b => b.SellerId == id && b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Get first 5 reviews for this seller
            var reviews = await _context.Reviews
                .Include(r => r.Buyer)
                .Include(r => r.Book)
                .Where(r => r.SellerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            var viewModel = new SellerProfileViewModel
            {
                Seller = seller,
                Books = books,
                Reviews = reviews,
                HasMoreReviews = seller.TotalReviews > 5
            };

            return View(viewModel);
        }

        // API: Load more reviews for a seller
        [HttpGet]
        public async Task<IActionResult> LoadMoreReviews(int sellerId, int skip = 0, int take = 5)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Buyer)
                .Include(r => r.Book)
                .Where(r => r.SellerId == sellerId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(r => new
                {
                    buyerName = r.Buyer!.FullName,
                    rating = r.Rating,
                    comment = r.Comment,
                    bookTitle = r.Book!.Title,
                    createdAt = r.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(reviews);
        }
    }

    public class SellerProfileViewModel
    {
        public User Seller { get; set; } = null!;
        public List<Book> Books { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public bool HasMoreReviews { get; set; }
    }
}
