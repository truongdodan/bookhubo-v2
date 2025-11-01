using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHubo.Models
{
    [Table("reviews")]
    public class Review
    {
        [Key]
        [Column("reviewid")]
        public int ReviewId { get; set; }

        [Required]
        [Column("orderitemid")]
        public int OrderItemId { get; set; }

        [Required]
        [Column("buyerid")]
        public int BuyerId { get; set; }

        [Required]
        [Column("sellerid")]
        public int SellerId { get; set; }

        [Required]
        [Column("bookid")]
        public int BookId { get; set; }

        [Required]
        [Range(1, 5)]
        [Column("rating")]
        public int Rating { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderItemId")]
        public OrderItem? OrderItem { get; set; }

        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }
    }
}
