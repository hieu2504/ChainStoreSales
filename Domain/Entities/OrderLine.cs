using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class OrderLine
    {
        public long OrderLineId { get; set; }
        
        public long OrderId { get; set; }
        
        public long VariantId { get; set; }
        
        public int Qty { get; set; } // > 0
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineDiscount { get; set; } = 0;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Amount { get; set; } // Computed: UnitPrice * Qty - LineDiscount
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Order Order { get; set; } = null!;
        public ProductVariant ProductVariant { get; set; } = null!;
        public ICollection<OrderLineSerial> OrderLineSerials { get; set; } = new List<OrderLineSerial>();
    }
}
