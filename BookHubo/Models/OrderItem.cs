using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHubo.Models
{
    [Table("orderitems")]
    public class OrderItem
    {
        [Key]
        [Column("orderitemid")]
        public int OrderItemId { get; set; }

        [Required]
        [Column("orderid")]
        public int OrderId { get; set; }

        [Required]
        [Column("bookid")]
        public int BookId { get; set; }

        [Required]
        [Column("sellerid")]
        public int SellerId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("priceatpurchase")]
        public decimal PriceAtPurchase { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Completed, Cancelled

        [Column("shippedat")]
        public DateTime? ShippedAt { get; set; }

        [Column("completedat")]
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [ForeignKey("SellerId")]
        public User? Seller { get; set; }
    }
}
