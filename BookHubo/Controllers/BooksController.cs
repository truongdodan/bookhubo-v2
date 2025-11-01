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
    }
}
