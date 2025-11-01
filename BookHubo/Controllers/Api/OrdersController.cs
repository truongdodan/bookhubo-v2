using BookHubo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookHubo.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BookHubDbContext _context;

        public OrdersController(BookHubDbContext context)
        {
            _context = context;
        }

        // PUT: api/Orders/MarkShipped/{orderItemId}
        [HttpPut("MarkShipped/{orderItemId}")]
        public async Task<IActionResult> MarkShipped(int orderItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Check if user is the seller
            if (orderItem.SellerId != userId.Value)
            {
                return Forbid();
            }

            // Check if order is in Pending status
            if (orderItem.Status != "Pending")
            {
                return BadRequest(new { success = false, message = "Chỉ có thể đánh dấu đơn hàng đang Pending" });
            }

            orderItem.Status = "Shipped";
            orderItem.ShippedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã đánh dấu đơn hàng là Shipped" });
        }

        // PUT: api/Orders/MarkCompleted/{orderItemId}
        [HttpPut("MarkCompleted/{orderItemId}")]
        public async Task<IActionResult> MarkCompleted(int orderItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);

            if (orderItem == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Check if user is the buyer
            if (orderItem.Order?.BuyerId != userId.Value)
            {
                return Forbid();
            }

            // Check if order is in Shipped status
            if (orderItem.Status != "Shipped")
            {
                return BadRequest(new { success = false, message = "Chỉ có thể xác nhận đơn hàng đã Shipped" });
            }

            orderItem.Status = "Completed";
            orderItem.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xác nhận nhận hàng thành công" });
        }
    }
}
