using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class ProductVariant
    {
        public long VariantId { get; set; }
        
        public long ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Sku { get; set; } = string.Empty;
        
        [StringLength(128)]
        public string? Barcode { get; set; }
        
        [StringLength(300)]
        public string? OptionSummary { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }
        
        public bool TrackSerial { get; set; } = false;
        
        public bool IsDeleted { get; set; } = false;
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Product Product { get; set; } = null!;
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InventorySerial> InventorySerials { get; set; } = new List<InventorySerial>();
        public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
