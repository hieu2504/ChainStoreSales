using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Coupon
    {
        public long CouponId { get; set; }
        
        public long ShopId { get; set; }
        
        [Required]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string Type { get; set; } = string.Empty; // ORDER_PERCENT/ORDER_AMOUNT/ITEM_PERCENT/BUY_X_GET_Y
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Value { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }
        
        public DateTime StartAt { get; set; }
        
        public DateTime EndAt { get; set; }
        
        public int? MaxRedemptions { get; set; }
        
        public int? PerCustomerLimit { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
        public ICollection<CouponRedemption> CouponRedemptions { get; set; } = new List<CouponRedemption>();
        public ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
    }
}
