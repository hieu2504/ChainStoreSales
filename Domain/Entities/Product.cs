using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Product
    {
        public long ProductId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn cửa hàng")]
        public long? ShopId { get; set; }
        
        public long? CategoryId { get; set; }
        
        public string? Code { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(300, ErrorMessage = "Tên sản phẩm không được vượt quá 300 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        public int? BrandId { get; set; }
        
        public bool CanSell { get; set; } = true;
        
        [StringLength(50)]
        public string Unit { get; set; } = "Cái";
        
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        
        // Fields from ProductVariant
        [Required]
        [StringLength(100)]
        public string Sku { get; set; } = string.Empty;
        
        [StringLength(128)]
        public string? Barcode { get; set; }
        
        [StringLength(300)]
        public string? OptionSummary { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        // TrackSerial = true dùng để theo dõi số seri của sản phẩm trong kho (InventorySerial)
        // Nếu TrackSerial = true thì sản phẩm này sẽ có số seri
        // Nếu TrackSerial = false thì sản phẩm này sẽ không có số seri
        public bool TrackSerial { get; set; } = false;
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Shop? Shop { get; set; } = null!;
        public ProductCategory? Category { get; set; }
        public Brand? Brand { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InventorySerial> InventorySerials { get; set; } = new List<InventorySerial>();
        public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
