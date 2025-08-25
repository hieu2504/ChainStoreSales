using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Customer
    {
        public long CustomerId { get; set; }
        
        public long ShopId { get; set; }
        
        [StringLength(50)]
        public string? Code { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;
        
        [StringLength(30)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Email { get; set; }
        
        [StringLength(50)]
        public string? TaxCode { get; set; }
        
        public byte? Gender { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        [StringLength(1000)]
        public string? Note { get; set; }
        
        public bool IsArchived { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public byte[] RowVersion { get; set; } = new byte[0];

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CouponRedemption> CouponRedemptions { get; set; } = new List<CouponRedemption>();
    }
}
