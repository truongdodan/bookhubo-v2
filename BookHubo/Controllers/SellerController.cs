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

            // Get reviews for this seller
            var reviews = await _context.Reviews
                .Include(r => r.Buyer)
                .Include(r => r.OrderItem)
                    .ThenInclude(oi => oi!.Book)
                .Where(r => r.SellerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var viewModel = new SellerProfileViewModel
            {
                Seller = seller,
                Books = books,
                Reviews = reviews
            };

            return View(viewModel);
        }
    }

    public class SellerProfileViewModel
    {
        public User Seller { get; set; } = null!;
        public List<Book> Books { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
