using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class ProductImage
    {
        public long ImageId { get; set; }
        
        public long ProductId { get; set; }
        
        [Required]
        [StringLength(400)]
        public string FilePath { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? AltText { get; set; }
        
        public int SortOrder { get; set; } = 0;
        
        public bool IsPrimary { get; set; } = false;
        
        public bool IsDeleted { get; set; } = false;
        
        public byte[] RowVersion { get; set; } = new byte[0];
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Product Product { get; set; } = null!;    
    }
}
