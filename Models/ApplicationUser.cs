using Microsoft.AspNetCore.Identity;

namespace ChainStoreSalesManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public long? CurrentShopId { get; set; }
        public long? CurrentBranchId { get; set; }
    }
}
