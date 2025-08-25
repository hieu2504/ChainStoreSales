using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities.Views
{
    public class RevenueDaily
    {
        public long ShopId { get; set; }
        public long BranchId { get; set; }
        public DateTime SaleDate { get; set; }
        public int Orders { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSales { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSales { get; set; }
    }
}
