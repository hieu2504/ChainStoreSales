using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ChainStoreSalesManagement.Controllers
{
    [Authorize]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Brands
        public async Task<IActionResult> Index(string searchTerm, int? shopId, int page = 1, int pageSize = 8)
        {
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentShopId = shopId;

            var brandsQuery = _context.Brands
                .Include(b => b.Shop)
                .Include(b => b.Products)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                brandsQuery = brandsQuery.Where(b => b.Name.Contains(searchTerm));
            }

            // Apply shop filter
            if (shopId.HasValue)
            {
                brandsQuery = brandsQuery.Where(b => b.ShopId == shopId.Value);
            }

            // Count total items
            var totalItems = await brandsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Apply pagination
            var brands = await brandsQuery
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get shops for filter dropdown
            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .Select(s => new { s.ShopId, s.Name })
                .ToListAsync();

            // Pass pagination data to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalItems);
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            return View(brands);
        }

        // GET: Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .Include(b => b.Shop)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BrandId == id);
                
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Brands/Create
        public async Task<IActionResult> Create()
        {
            // Get shops for dropdown
            ViewBag.ShopId = new SelectList(
                await _context.Shops
                    .Where(s => s.IsActive)
                    .Select(s => new { s.ShopId, s.Name })
                    .ToListAsync(),
                "ShopId",
                "Name"
            );
                
            return View();
        }

        // POST: Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShopId,Name")] Brand brand)
        {
            if (ModelState.IsValid)
            {
                // Check if name already exists for this shop
                var existingBrand = await _context.Brands
                    .AnyAsync(b => b.ShopId == brand.ShopId && b.Name == brand.Name);
                    
                if (existingBrand)
                {
                    ModelState.AddModelError("Name", "Tên thương hiệu đã tồn tại trong cửa hàng này.");
                }
                else
                {
                    _context.Add(brand);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm thương hiệu thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Reload shops for dropdown if validation fails
            ViewBag.ShopId = new SelectList(
                await _context.Shops
                    .Where(s => s.IsActive)
                    .Select(s => new { s.ShopId, s.Name })
                    .ToListAsync(),
                "ShopId",
                "Name",
                brand.ShopId
            );
                
            return View(brand);
        }

        // GET: Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            
            // Get shops for dropdown
            ViewBag.ShopId = new SelectList(
                await _context.Shops
                    .Where(s => s.IsActive)
                    .Select(s => new { s.ShopId, s.Name })
                    .ToListAsync(),
                "ShopId",
                "Name",
                brand.ShopId
            );
                
            return View(brand);
        }

        // POST: Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BrandId,ShopId,Name")] Brand brand)
        {
            if (id != brand.BrandId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if name already exists for this shop (excluding current brand)
                    var existingBrand = await _context.Brands
                        .AnyAsync(b => b.ShopId == brand.ShopId && 
                                      b.Name == brand.Name && 
                                      b.BrandId != brand.BrandId);
                        
                    if (existingBrand)
                    {
                        ModelState.AddModelError("Name", "Tên thương hiệu đã tồn tại trong cửa hàng này.");
                    }
                    else
                    {
                        _context.Update(brand);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật thương hiệu thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.BrandId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            // Reload shops for dropdown if validation fails
            ViewBag.ShopId = new SelectList(
                await _context.Shops
                    .Where(s => s.IsActive)
                    .Select(s => new { s.ShopId, s.Name })
                    .ToListAsync(),
                "ShopId",
                "Name",
                brand.ShopId
            );
                
            return View(brand);
        }

        // GET: Brands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .Include(b => b.Shop)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BrandId == id);
                
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.BrandId == id);
                
            if (brand != null)
            {
                // Check if brand has any products
                if (brand.Products.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa thương hiệu này vì có sản phẩm đang sử dụng.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
                
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thương hiệu thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.BrandId == id);
        }
    }
}
