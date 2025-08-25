using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class ProductCategory
    {
        public long CategoryId { get; set; }
        
        public long? ShopId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(1000)]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }

        // Navigation properties
        public Shop? Shop { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
