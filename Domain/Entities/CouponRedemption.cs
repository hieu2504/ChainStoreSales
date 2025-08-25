namespace ChainStoreSalesManagement.Domain.Entities
{
    public class CouponRedemption
    {
        public long RedemptionId { get; set; }
        
        public long CouponId { get; set; }
        
        public long? OrderId { get; set; }
        
        public long? CustomerId { get; set; }
        
        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Coupon Coupon { get; set; } = null!;
        public Order? Order { get; set; }
        public Customer? Customer { get; set; }
    }
}
