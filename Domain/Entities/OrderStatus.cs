using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class OrderStatus
    {
        [Key]
        [StringLength(30)]
        public string StatusCode { get; set; } = string.Empty;
        
        public int Rank { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
