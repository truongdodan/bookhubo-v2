using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHubo.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("userid")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [Column("passwordhash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Column("fullname")]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Column("phonenumber")]
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        [Column("shippingaddress")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = "User";

        [Column("averagerating")]
        public decimal AverageRating { get; set; } = 0.00m;

        [Column("totalreviews")]
        public int TotalReviews { get; set; } = 0;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
