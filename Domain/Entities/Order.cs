using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Order
    {
        public long OrderId { get; set; }
        
        public long ShopId { get; set; }
        
        public long BranchId { get; set; }
        
        [Required]
        [StringLength(30)]
        public string OrderNo { get; set; } = string.Empty;
        
        public long? CustomerId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalAmount { get; set; } // Computed: SubTotal - Discount + ShippingFee + Tax
        
        [StringLength(1000)]
        public string? Note { get; set; }
        
        public string? SalesUserId { get; set; }
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public Customer? Customer { get; set; }
        public OrderStatus OrderStatus { get; set; } = null!;
        public ApplicationUser? SalesUser { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<CouponRedemption> CouponRedemptions { get; set; } = new List<CouponRedemption>();
    }
}
