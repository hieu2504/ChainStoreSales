using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class PaymentMethod
    {
        [Key]
        [StringLength(30)]
        public string MethodCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
