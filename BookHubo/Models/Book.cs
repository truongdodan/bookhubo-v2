using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHubo.Models
{
    [Table("books")]
    public class Book
    {
        [Key]
        [Column("bookid")]
        public int BookId { get; set; }

        [Required]
        [Column("sellerid")]
        public int SellerId { get; set; }

        [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
        [Column("title")]
        [MaxLength(500)]
        [Display(Name = "Tiêu đề sách")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả là bắt buộc")]
        [Column("author")]
        [MaxLength(255)]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [Column("isbn")]
        [MaxLength(50)]
        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Column("description")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Column("price")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Column("category")]
        [MaxLength(100)]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tình trạng sách là bắt buộc")]
        [Column("condition")]
        [MaxLength(50)]
        [Display(Name = "Tình trạng")]
        public string Condition { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Column("stockquantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải >= 0")]
        [Display(Name = "Số lượng")]
        public int StockQuantity { get; set; } = 1;

        [Column("imagepath")]
        [MaxLength(500)]
        public string? ImagePath { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("averagerating")]
        public decimal AverageRating { get; set; } = 0;

        [Column("totalreviews")]
        public int TotalReviews { get; set; } = 0;

        // Navigation property
        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}
