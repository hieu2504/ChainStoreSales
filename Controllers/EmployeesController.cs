using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;
using System.Text.Json;

namespace ChainStoreSalesManagement.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string search, long? branchId, bool? isActive, int page = 1, int pageSize = 20)
        {
            ViewBag.Search = search;
            ViewBag.BranchId = branchId;
            ViewBag.IsActive = isActive;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Shop)
                .Include(e => e.Branch)
                .AsQueryable();

            // Search by User Name, Email, or Employee Title
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => 
                    (e.User.UserName != null && e.User.UserName.Contains(search)) ||
                    (e.User.Email != null && e.User.Email.Contains(search)) ||
                    (e.Title != null && e.Title.Contains(search)));
            }

            // Filter by Branch
            if (branchId.HasValue)
            {
                query = query.Where(e => e.BranchId == branchId.Value);
            }

            // Filter by Active Status
            if (isActive.HasValue)
            {
                query = query.Where(e => e.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPages;
            ViewBag.StartItem = totalCount == 0 ? 0 : (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalCount);

            var employees = await query
                .OrderBy(e => e.User.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get branches for filter dropdown
            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Shop)
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            // Get sales statistics
            var salesCount = await _context.Orders
                .Where(o => o.SalesUserId == employee.UserId)
                .CountAsync();
            
            ViewBag.SalesCount = salesCount;

            return View(employee);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
                
            // Get available users who are not already employees
            ViewBag.AvailableUsers = await _context.Users
                .Where(u => !_context.Employees.Any(e => e.UserId == u.Id))
                .Select(u => new {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .OrderBy(u => u.UserName)
                .ToListAsync();
                
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,BranchId,Title,HiredDate,IsActive")] Employee employee)
        {
            // Get current shop ID (assume from session/context)
            employee.ShopId = 1; // TODO: Get from current user context
            
            var validationResult = await ValidateEmployeeAsync(employee);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            if (ModelState.IsValid)
            {
                employee.CreatedAt = DateTime.UtcNow;
                _context.Add(employee);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
                
            // Get available users who are not already employees
            ViewBag.AvailableUsers = await _context.Users
                .Where(u => !_context.Employees.Any(e => e.UserId == u.Id))
                .Select(u => new {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .OrderBy(u => u.UserName)
                .ToListAsync();
                
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("EmployeeId,UserId,BranchId,Title,HiredDate,IsActive,ShopId,CreatedAt")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            var validationResult = await ValidateEmployeeAsync(employee, id);
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
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EmployeeExistsAsync(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            var employeeWithUser = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employeeWithUser != null)
            {
                employee.User = employeeWithUser.User;
            }

            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Shop)
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            // Check if employee has sales orders
            var hasSales = await _context.Orders
                .AnyAsync(o => o.SalesUserId == employee.UserId);
                
            ViewBag.HasSales = hasSales;
            
            // Get sales statistics
            var salesCount = await _context.Orders
                .Where(o => o.SalesUserId == employee.UserId)
                .CountAsync();
                
            ViewBag.SalesCount = salesCount;

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Check if employee has sales orders
            var hasSales = await _context.Orders
                .AnyAsync(o => o.SalesUserId == employee.UserId);

            if (hasSales)
            {
                // If has sales, just deactivate
                employee.IsActive = false;
                _context.Update(employee);
                TempData["SuccessMessage"] = "Nhân viên đã được vô hiệu hóa do có dữ liệu bán hàng.";
            }
            else
            {
                // If no sales, can delete completely
                _context.Employees.Remove(employee);
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API for user typeahead
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var users = await _context.Users
                .Where(u => (u.UserName != null && u.UserName.Contains(term)) || 
                           (u.Email != null && u.Email.Contains(term)))
                .Where(u => !_context.Employees.Any(e => e.UserId == u.Id)) // Not already an employee
                .Select(u => new {
                    id = u.Id,
                    text = $"{u.UserName} ({u.Email})",
                    email = u.Email
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }

        private async Task<ValidationResult> ValidateEmployeeAsync(Employee employee, long? excludeId = null)
        {
            var result = new ValidationResult { IsValid = true };

            // Check unique constraint: ShopId + UserId
            var existingEmployee = await _context.Employees
                .Where(e => e.ShopId == employee.ShopId && e.UserId == employee.UserId)
                .Where(e => !excludeId.HasValue || e.EmployeeId != excludeId.Value)
                .FirstOrDefaultAsync();

            if (existingEmployee != null)
            {
                result.IsValid = false;
                result.Errors.Add("UserId", "Người dùng này đã là nhân viên của cửa hàng.");
            }

            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == employee.UserId);
            if (!userExists)
            {
                result.IsValid = false;
                result.Errors.Add("UserId", "Người dùng không tồn tại.");
            }

            return result;
        }

        private async Task<bool> EmployeeExistsAsync(long id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == id);
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
        }
    }
}
