using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class BillingTransaction
    {
        public long BillId { get; set; }
        
        public long ShopSubId { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime PeriodFrom { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime PeriodTo { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        public DateTime? PaidAt { get; set; }
        
        [StringLength(100)]
        public string? PaymentRef { get; set; }
        
        [StringLength(30)]
        public string? Method { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "PENDING";

        // Navigation properties
        public ShopSubscription ShopSubscription { get; set; } = null!;
    }
}
