namespace ChainStoreSalesManagement.Domain.Entities
{
    public class CouponProduct
    {
        public long CouponId { get; set; }
        
        public long ProductId { get; set; }

        // Navigation properties
        public Coupon Coupon { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
