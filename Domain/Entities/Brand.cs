using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Brand
    {
        public int BrandId { get; set; }
        
        public long ShopId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
