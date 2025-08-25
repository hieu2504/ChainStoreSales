using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class ErrorLog
    {
        public long LogId { get; set; }
        
        public DateTime LogTime { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(20)]
        public string Level { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Source { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        public string? Exception { get; set; }
        
        public string? UserId { get; set; }
        
        public long? ShopId { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Shop? Shop { get; set; }
    }
}
