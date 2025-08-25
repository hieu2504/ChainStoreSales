using System.ComponentModel.DataAnnotations.Schema;

namespace ChainStoreSalesManagement.Domain.Entities.Views
{
    public class PersonalSales
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public int Orders { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSales { get; set; }
    }
}
