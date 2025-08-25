using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Payment
    {
        public long PaymentId { get; set; }
        
        public long OrderId { get; set; }
        
        [Required]
        [StringLength(30)]
        public string MethodCode { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }
        
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? RefNo { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; } = null!;
    }
}
