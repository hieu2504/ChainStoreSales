using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class SubscriptionPlan
    {
        public int PlanId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerMonth { get; set; }
        
        public int? MaxUsers { get; set; }
        
        public int? MaxBranches { get; set; }
        
        public string? FeaturesJson { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<ShopSubscription> ShopSubscriptions { get; set; } = new List<ShopSubscription>();
    }
}
