using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        
        public long ShopId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public long? BranchId { get; set; }
        
        [StringLength(100)]
        public string? Title { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime? HiredDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public Branch? Branch { get; set; }
    }
}
