using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookHubDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BooksController(BookHubDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Books - My Listings
        public async Task<IActionResult> Index(string filter = "all")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Book> query = _context.Books.Where(b => b.SellerId == userId.Value);

            // Filter
            if (filter == "active")
            {
                query = query.Where(b => b.IsActive);
            }
            else if (filter == "sold")
            {
                query = query.Where(b => !b.IsActive);
            }

            var books = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            // Quick stats
            ViewBag.TotalActive = await _context.Books.CountAsync(b => b.SellerId == userId.Value && b.IsActive);
            ViewBag.TotalSold = await _context.Books.CountAsync(b => b.SellerId == userId.Value && !b.IsActive);
            ViewBag.CurrentFilter = filter;

            return View(books);
        }

        // GET: /Books/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: /Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? image)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Remove Seller from validation
            ModelState.Remove("Seller");
            ModelState.Remove("image");

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            // Validate image is required
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("", "Hình ảnh sách là bắt buộc");
                return View(book);
            }

            // Save image
            string? imagePath = await SaveImageAsync(image);
            if (imagePath == null)
            {
                ModelState.AddModelError("", "Có lỗi khi upload hình ảnh");
                return View(book);
            }

            book.SellerId = userId.Value;
            book.ImagePath = imagePath;
            book.CreatedAt = DateTime.UtcNow;
            book.IsActive = true;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Books/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null || book.SellerId != userId.Value)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? image)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != book.BookId)
            {
                return NotFound();
            }

            var existingBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == id);
            if (existingBook == null || existingBook.SellerId != userId.Value)
            {
                return NotFound();
            }

            // Remove từ validation
            ModelState.Remove("Seller");
            ModelState.Remove("image");

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            // Nếu có upload image mới
            if (image != null && image.Length > 0)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existingBook.ImagePath))
                {
                    DeleteImage(existingBook.ImagePath);
                }

                // Save ảnh mới
                string? imagePath = await SaveImageAsync(image);
                if (imagePath != null)
                {
                    book.ImagePath = imagePath;
                }
            }
            else
            {
                // Giữ ảnh cũ
                book.ImagePath = existingBook.ImagePath;
            }

            book.SellerId = userId.Value;
            book.CreatedAt = existingBook.CreatedAt;

            _context.Update(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Books/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null || book.SellerId != userId.Value)
            {
                return NotFound();
            }

            // Soft delete
            book.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<string?> SaveImageAsync(IFormFile image)
        {
            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "books");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return "/images/books/" + uniqueFileName;
            }
            catch
            {
                return null;
            }
        }

        private void DeleteImage(string imagePath)
        {
            try
            {
                string fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        // GET: /Books/Category/{category}
        public async Task<IActionResult> Category(string category)
        {
            var books = await _context.Books
                .Include(b => b.Seller)
                .Where(b => b.IsActive && b.Category == category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.Category = category;
            return View(books);
        }

        // GET: /Books/Search
        public async Task<IActionResult> Search(string keyword, string category, string condition,
            decimal? minPrice, decimal? maxPrice, string sortBy = "newest")
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Seller)
                .Where(b => b.IsActive);

            // Search keyword in Title and Author
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(b => b.Title.Contains(keyword) || b.Author.Contains(keyword));
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category == category);
            }

            // Filter by condition
            if (!string.IsNullOrWhiteSpace(condition))
            {
                query = query.Where(b => b.Condition == condition);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            // Sort
            query = sortBy switch
            {
                "price-asc" => query.OrderBy(b => b.Price),
                "price-desc" => query.OrderByDescending(b => b.Price),
                _ => query.OrderByDescending(b => b.CreatedAt) // newest (default)
            };

            var books = await query.ToListAsync();

            // Pass search params to view
            ViewBag.Keyword = keyword;
            ViewBag.Category = category;
            ViewBag.Condition = condition;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;

            return View(books);
        }

        // GET: /Books/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Seller)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Get related books (same category, exclude current)
            var relatedBooks = await _context.Books
                .Include(b => b.Seller)
                .Where(b => b.IsActive && b.Category == book.Category && b.BookId != id)
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedBooks = relatedBooks;

            return View(book);
        }
    }
}
