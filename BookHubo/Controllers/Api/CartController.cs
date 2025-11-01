using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly BookHubDbContext _context;

        public CartController(BookHubDbContext context)
        {
            _context = context;
        }

        // POST: api/Cart/Add
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
            }

            // Check book exists and has stock
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null || !book.IsActive)
            {
                return NotFound(new { success = false, message = "Sách không tồn tại hoặc đã ngừng bán" });
            }

            // Check if user is trying to buy their own book
            if (book.SellerId == userId.Value)
            {
                return BadRequest(new { success = false, message = "Bạn không thể mua sách của chính mình" });
            }

            if (book.StockQuantity <= 0)
            {
                return BadRequest(new { success = false, message = "Sách đã hết hàng" });
            }

            // Check if item already in cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId.Value && ci.BookId == request.BookId);

            if (existingItem != null)
            {
                // Increase quantity
                if (existingItem.Quantity + 1 > book.StockQuantity)
                {
                    return BadRequest(new { success = false, message = "Số lượng vượt quá tồn kho" });
                }

                existingItem.Quantity++;
                existingItem.AddedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new cart item
                var cartItem = new CartItem
                {
                    UserId = userId.Value,
                    BookId = request.BookId,
                    Quantity = 1,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Get cart count
            var cartCount = await _context.CartItems
                .Where(ci => ci.UserId == userId.Value)
                .SumAsync(ci => ci.Quantity);

            return Ok(new { success = true, cartCount, message = "Đã thêm vào giỏ hàng" });
        }

        // DELETE: api/Cart/Remove/{cartItemId}
        [HttpDelete("Remove/{cartItemId}")]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId.Value);

            if (cartItem == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            // Get cart count
            var cartCount = await _context.CartItems
                .Where(ci => ci.UserId == userId.Value)
                .SumAsync(ci => ci.Quantity);

            return Ok(new { success = true, cartCount, message = "Đã xóa khỏi giỏ hàng" });
        }

        // PUT: api/Cart/UpdateQuantity
        [HttpPut("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.CartItemId == request.CartItemId && ci.UserId == userId.Value);

            if (cartItem == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            if (request.Quantity > cartItem.Book.StockQuantity)
            {
                return BadRequest(new { success = false, message = "Số lượng vượt quá tồn kho" });
            }

            cartItem.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            var newSubtotal = cartItem.Quantity * cartItem.Book.Price;

            return Ok(new { success = true, newSubtotal });
        }

        // GET: api/Cart/Count
        [HttpGet("Count")]
        public async Task<IActionResult> GetCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Ok(new { count = 0 });
            }

            var count = await _context.CartItems
                .Where(ci => ci.UserId == userId.Value)
                .SumAsync(ci => ci.Quantity);

            return Ok(new { count });
        }
    }

    public class AddToCartRequest
    {
        public int BookId { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
