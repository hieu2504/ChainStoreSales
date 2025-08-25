using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities.Views
{
    public class PaymentHistory
    {
        public long PaymentId { get; set; }
        public long OrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public string MethodCode { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }
        
        public DateTime PaidAt { get; set; }
        public long ShopId { get; set; }
        public long BranchId { get; set; }
        public long? CustomerId { get; set; }
    }
}
