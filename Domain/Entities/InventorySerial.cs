using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class InventorySerial
    {
        public long SerialId { get; set; }
        
        public long ShopId { get; set; }
        
        public long BranchId { get; set; }
        
        public long ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SerialNo { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Status { get; set; } = "ON_HAND";

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
