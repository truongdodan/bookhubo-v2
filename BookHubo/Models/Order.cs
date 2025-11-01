using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHubo.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("orderid")]
        public int OrderId { get; set; }

        [Required]
        [Column("buyerid")]
        public int BuyerId { get; set; }

        [Required]
        [Column("totalprice")]
        public decimal TotalPrice { get; set; }

        [Required]
        [Column("shippingaddress")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Column("orderdate")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
