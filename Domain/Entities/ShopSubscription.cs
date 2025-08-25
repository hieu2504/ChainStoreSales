using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class ShopSubscription
    {
        public long ShopSubId { get; set; }
        
        public long ShopId { get; set; }
        
        public int PlanId { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
        
        public bool AutoRenew { get; set; } = true;
        
        public byte Status { get; set; } = 1;

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public ICollection<BillingTransaction> BillingTransactions { get; set; } = new List<BillingTransaction>();
    }
}
