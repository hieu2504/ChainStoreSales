using System.ComponentModel.DataAnnotations;

namespace ChainStoreSalesManagement.Domain.Entities
{
    public class Brand
    {
        public int BrandId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn cửa hàng")]
        public long ShopId { get; set; }
        
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(200, ErrorMessage = "Tên thương hiệu không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public Shop? Shop { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
