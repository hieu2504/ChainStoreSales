using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Shop
    {
        public long ShopId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        
        public string? OwnerUserId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser? OwnerUser { get; set; }
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<Brand> Brands { get; set; } = new List<Brand>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InventorySerial> InventorySerials { get; set; } = new List<InventorySerial>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ShopSubscription> ShopSubscriptions { get; set; } = new List<ShopSubscription>();
        public ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();
    }
}
