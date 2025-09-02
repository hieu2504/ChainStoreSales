using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ChainStoreSalesManagement.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShopController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Shop
        public async Task<IActionResult> Index(string searchName = "", string searchCode = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Shops
                .Include(s => s.OwnerUser)
                .AsQueryable();

            // Search filters
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(s => s.Name.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchCode))
            {
                query = query.Where(s => s.Code.Contains(searchCode));
            }

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var shops = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Setup ViewBag for pagination and search
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchName = searchName;
            ViewBag.SearchCode = searchCode;

            return View(shops);
        }

        // GET: Shop/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.OwnerUser)
                .FirstOrDefaultAsync(m => m.ShopId == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        // GET: Shop/Create
        public IActionResult Create()
        {
            // Get users for dropdown
            ViewBag.OwnerUserId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _userManager.Users.ToList(), 
                "Id", 
                "UserName", 
                null
            );

            return View();
        }

        // POST: Shop/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,Address,OwnerUserId,IsActive")] Shop shop)
        {
            // Check for unique code
            if (await _context.Shops.AnyAsync(s => s.Code == shop.Code))
            {
                ModelState.AddModelError("Code", "Mã cửa hàng đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                shop.CreatedAt = DateTime.UtcNow;
                _context.Add(shop);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo cửa hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Reload users for dropdown on error
            ViewBag.OwnerUserId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _userManager.Users.ToList(), 
                "Id", 
                "UserName", 
                shop.OwnerUserId
            );

            return View(shop);
        }

        // GET: Shop/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }

            // Get users for dropdown
            ViewBag.OwnerUserId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _userManager.Users.ToList(), 
                "Id", 
                "UserName", 
                shop.OwnerUserId
            );

            return View(shop);
        }

        // POST: Shop/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ShopId,Code,Name,Address,OwnerUserId,IsActive,CreatedAt")] Shop shop)
        {
            if (id != shop.ShopId)
            {
                return NotFound();
            }

            // Check for unique code (excluding current shop)
            if (await _context.Shops.AnyAsync(s => s.Code == shop.Code && s.ShopId != shop.ShopId))
            {
                ModelState.AddModelError("Code", "Mã cửa hàng đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật cửa hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.ShopId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Reload users for dropdown on error
            ViewBag.OwnerUserId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _userManager.Users.ToList(), 
                "Id", 
                "UserName", 
                shop.OwnerUserId
            );

            return View(shop);
        }

        // GET: Shop/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.OwnerUser)
                .FirstOrDefaultAsync(m => m.ShopId == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        // POST: Shop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var shop = await _context.Shops
                .Include(s => s.Branches)
                .Include(s => s.ProductCategories)
                .Include(s => s.Brands)
                .Include(s => s.Products)
                .Include(s => s.Employees)
                .Include(s => s.Customers)
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.ShopId == id);
                
            if (shop != null)
            {
                // Check if shop has any references
                var referenceCount = shop.Branches.Count + 
                                   shop.ProductCategories.Count + 
                                   shop.Brands.Count + 
                                   shop.Products.Count + 
                                   shop.Employees.Count + 
                                   shop.Customers.Count + 
                                   shop.Orders.Count;

                if (referenceCount > 0)
                {
                    TempData["ErrorMessage"] = "Không thể xóa cửa hàng này vì có dữ liệu liên quan đang sử dụng.";
                    return RedirectToAction(nameof(Index));
                }
                
                _context.Shops.Remove(shop);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa cửa hàng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ShopExists(long id)
        {
            return _context.Shops.Any(e => e.ShopId == id);
        }
    }
}
