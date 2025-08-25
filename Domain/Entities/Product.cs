using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Product
    {
        public long ProductId { get; set; }
        
        public long ShopId { get; set; }
        
        public long? CategoryId { get; set; }
        
        public string? Code { get; set; }
        
        [Required]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;
        
        public int? BrandId { get; set; }
        
        public bool CanSell { get; set; } = true;
        
        [StringLength(50)]
        public string Unit { get; set; } = "Cái";
        
        public string? Description { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public ProductCategory? Category { get; set; }
        public Brand? Brand { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
