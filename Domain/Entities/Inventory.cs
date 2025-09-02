namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Inventory
    {
        public long ShopId { get; set; }
        
        public long BranchId { get; set; }
        
        public long ProductId { get; set; }
        
        public int OnHand { get; set; } = 0;
        
        public int Allocated { get; set; } = 0;
        
        public int InTransit { get; set; } = 0;

        // Navigation properties
        public Shop Shop { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
