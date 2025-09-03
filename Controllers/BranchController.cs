using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;

namespace ChainStoreSalesManagement.Controllers
{
    public class BranchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Branch
        public async Task<IActionResult> Index(string search, long? shopId, int page = 1, int pageSize = 20)
        {
            ViewBag.Search = search;
            ViewBag.ShopId = shopId;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var query = _context.Branches
                .Include(b => b.Shop)
                .AsQueryable();

            // Search by Name or Address
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => 
                    b.Name.Contains(search) ||
                    (b.Address != null && b.Address.Contains(search)));
            }

            // Filter by Shop
            if (shopId.HasValue)
            {
                query = query.Where(b => b.ShopId == shopId.Value);
            }

            var totalCount = await query.CountAsync();
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPages;
            ViewBag.StartItem = totalCount == 0 ? 0 : (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalCount);

            var branches = await query
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get shops for filter dropdown
            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(branches);
        }

        // GET: Branch/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .Include(b => b.Shop)
                .FirstOrDefaultAsync(m => m.BranchId == id);
                
            if (branch == null)
            {
                return NotFound();
            }

            // Get statistics
            var employeeCount = await _context.Employees
                .Where(e => e.BranchId == id)
                .CountAsync();
            
            var orderCount = await _context.Orders
                .Where(o => o.BranchId == id)
                .CountAsync();

            ViewBag.EmployeeCount = employeeCount;
            ViewBag.OrderCount = orderCount;

            return View(branch);
        }

        // GET: Branch/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
                
            return View();
        }

        // POST: Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShopId,Code,Name,Address,Phone,TaxCode,IsActive")] Branch branch)
        {
            var validationResult = await ValidateBranchAsync(branch);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            if (ModelState.IsValid)
            {
                branch.CreatedAt = DateTime.UtcNow;
                _context.Add(branch);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm chi nhánh thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
                
            return View(branch);
        }

        // GET: Branch/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .Include(b => b.Shop)
                .FirstOrDefaultAsync(b => b.BranchId == id);
                
            if (branch == null)
            {
                return NotFound();
            }

            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(branch);
        }

        // POST: Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("BranchId,ShopId,Code,Name,Address,Phone,TaxCode,IsActive,CreatedAt,RowVersion")] Branch branch)
        {
            if (id != branch.BranchId)
            {
                return NotFound();
            }

            var validationResult = await ValidateBranchAsync(branch, id);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    branch.ModifiedAt = DateTime.UtcNow;
                    _context.Update(branch);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cập nhật chi nhánh thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BranchExistsAsync(branch.BranchId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var branchWithShop = await _context.Branches
                .Include(b => b.Shop)
                .FirstOrDefaultAsync(b => b.BranchId == id);
            if (branchWithShop != null)
            {
                branch.Shop = branchWithShop.Shop;
            }

            return View(branch);
        }

        // GET: Branch/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .Include(b => b.Shop)
                .FirstOrDefaultAsync(m => m.BranchId == id);
                
            if (branch == null)
            {
                return NotFound();
            }

            // Check if branch has related data
            var employeeCount = await _context.Employees
                .Where(e => e.BranchId == id)
                .CountAsync();
                
            var orderCount = await _context.Orders
                .Where(o => o.BranchId == id)
                .CountAsync();
                
            var inventoryCount = await _context.Inventories
                .Where(i => i.BranchId == id)
                .CountAsync();

            ViewBag.EmployeeCount = employeeCount;
            ViewBag.OrderCount = orderCount;
            ViewBag.InventoryCount = inventoryCount;
            ViewBag.HasRelatedData = employeeCount > 0 || orderCount > 0 || inventoryCount > 0;

            return View(branch);
        }

        // POST: Branch/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            // Check if branch has related data
            var hasRelatedData = await _context.Employees.AnyAsync(e => e.BranchId == id) ||
                                await _context.Orders.AnyAsync(o => o.BranchId == id) ||
                                await _context.Inventories.AnyAsync(i => i.BranchId == id);

            if (hasRelatedData)
            {
                // If has related data, just deactivate
                branch.IsActive = false;
                branch.ModifiedAt = DateTime.UtcNow;
                _context.Update(branch);
                TempData["SuccessMessage"] = "Chi nhánh đã được vô hiệu hóa do có dữ liệu liên quan.";
            }
            else
            {
                // If no related data, can delete completely
                _context.Branches.Remove(branch);
                TempData["SuccessMessage"] = "Xóa chi nhánh thành công!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<ValidationResult> ValidateBranchAsync(Branch branch, long? excludeId = null)
        {
            var result = new ValidationResult { IsValid = true };

            // Check unique constraint: ShopId + Code
            var existingBranch = await _context.Branches
                .Where(b => b.ShopId == branch.ShopId && b.Code == branch.Code)
                .Where(b => !excludeId.HasValue || b.BranchId != excludeId.Value)
                .FirstOrDefaultAsync();

            if (existingBranch != null)
            {
                result.IsValid = false;
                result.Errors.Add("Code", "Mã chi nhánh đã tồn tại trong cửa hàng này.");
            }

            // Check if shop exists
            var shopExists = await _context.Shops.AnyAsync(s => s.ShopId == branch.ShopId);
            if (!shopExists)
            {
                result.IsValid = false;
                result.Errors.Add("ShopId", "Cửa hàng không tồn tại.");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(branch.Code))
            {
                result.IsValid = false;
                result.Errors.Add("Code", "Mã chi nhánh không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(branch.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Name", "Tên chi nhánh không được để trống.");
            }

            return result;
        }

        private async Task<bool> BranchExistsAsync(long id)
        {
            return await _context.Branches.AnyAsync(e => e.BranchId == id);
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
        }
    }
}
