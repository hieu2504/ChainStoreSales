using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class OrderLineSerial
    {
        public long OrderLineSerialId { get; set; }
        
        public long OrderLineId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SerialNo { get; set; } = string.Empty;

        // Navigation properties
        public OrderLine OrderLine { get; set; } = null!;
    }
}
