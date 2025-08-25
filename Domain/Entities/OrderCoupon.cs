using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class OrderCoupon
    {
        public long OrderId { get; set; }
        
        public long CouponId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public Coupon Coupon { get; set; } = null!;
    }
}
