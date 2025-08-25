using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class RoleFeature
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;
        
        public int FeatureId { get; set; }
        
        public bool IsEnabled { get; set; } = true;

        // Navigation properties
        public IdentityRole Role { get; set; } = null!;
        public Feature Feature { get; set; } = null!;
    }
}
