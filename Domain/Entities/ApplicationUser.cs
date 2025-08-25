using Microsoft.AspNetCore.Identity;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public long? CurrentShopId { get; set; }
        public long? CurrentBranchId { get; set; }

        // Navigation properties
        public ICollection<Shop> OwnedShops { get; set; } = new List<Shop>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Order> SalesOrders { get; set; } = new List<Order>();
        public ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();
    }
}
