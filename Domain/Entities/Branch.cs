using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Branch
    {
        public long BranchId { get; set; }
        
        public long ShopId { get; set; }
        
        [Required]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        
        public string? Phone { get; set; }
        
        public string? TaxCode { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Shop? Shop { get; set; } = null!;
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InventorySerial> InventorySerials { get; set; } = new List<InventorySerial>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
