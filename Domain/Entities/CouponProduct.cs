namespace ChainStoreSalesManagement.Domain.Entities
{
    public class CouponProduct
    {
        public long CouponId { get; set; }
        
        public long VariantId { get; set; }

        // Navigation properties
        public Coupon Coupon { get; set; } = null!;
        public ProductVariant ProductVariant { get; set; } = null!;
    }
}
