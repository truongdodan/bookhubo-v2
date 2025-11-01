using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHubo.Models
{
    [Table("cartitems")]
    public class CartItem
    {
        [Key]
        [Column("cartitemid")]
        public int CartItemId { get; set; }

        [Required]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("bookid")]
        public int BookId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("addedat")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }
    }
}
