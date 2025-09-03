using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace ChainStoreSalesManagement.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string search, bool? hasTaxCode, int page = 1, int pageSize = 20)
        {
            ViewBag.Search = search;
            ViewBag.HasTaxCode = hasTaxCode;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var query = _context.Customers
                .Include(c => c.Shop)
                .Where(c => !c.IsArchived);

            // Search by Phone, Email, Code, or Name
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    (c.Phone != null && c.Phone.Contains(search)) ||
                    (c.Email != null && c.Email.Contains(search)) ||
                    (c.Code != null && c.Code.Contains(search)) ||
                    c.FullName.Contains(search));
            }

            // Filter by TaxCode presence
            if (hasTaxCode.HasValue)
            {
                if (hasTaxCode.Value)
                {
                    query = query.Where(c => !string.IsNullOrEmpty(c.TaxCode));
                }
                else
                {
                    query = query.Where(c => string.IsNullOrEmpty(c.TaxCode));
                }
            }

            var totalCount = await query.CountAsync();
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPages;
            ViewBag.StartItem = totalCount == 0 ? 0 : (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalCount);

            var customers = await query
                .OrderBy(c => c.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get shops for filter dropdown
            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Shop)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(m => m.CustomerId == id && !m.IsArchived);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var customer = new Customer
            {
                Code = await GenerateCustomerCodeAsync()
            };

            return View(customer);
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShopId,FullName,Phone,Email,TaxCode,Gender,BirthDate,Address,Note")] Customer customer)
        {
            // Generate customer code
            customer.Code = await GenerateCustomerCodeAsync();

            // Custom validations
            await ValidateCustomerAsync(customer, ModelState);

            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.UtcNow;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Khách hàng đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id && !c.IsArchived);

            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Shops = await _context.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("CustomerId,ShopId,Code,FullName,Phone,Email,TaxCode,Gender,BirthDate,Address,Note,CreatedAt,RowVersion")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            // Custom validations
            await ValidateCustomerAsync(customer, ModelState, isEdit: true);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerId == id);

                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // Update properties
                    existingCustomer.ShopId = customer.ShopId;
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.TaxCode = customer.TaxCode;
                    existingCustomer.Gender = customer.Gender;
                    existingCustomer.BirthDate = customer.BirthDate;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.Note = customer.Note;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Khách hàng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
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

            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Shop)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(m => m.CustomerId == id && !m.IsArchived);

            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.HasOrders = customer.Orders.Any();

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            // If customer has orders, archive instead of delete
            if (customer.Orders.Any())
            {
                customer.IsArchived = true;
                TempData["SuccessMessage"] = "Khách hàng đã được lưu trữ do có đơn hàng liên quan!";
            }
            else
            {
                _context.Customers.Remove(customer);
                TempData["SuccessMessage"] = "Khách hàng đã được xóa thành công!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper Methods
        private async Task<string> GenerateCustomerCodeAsync()
        {
            var today = DateTime.Now;
            var datePrefix = $"CUS-{today:yyMMdd}-";
            
            var lastCustomer = await _context.Customers
                .Where(c => c.Code != null && c.Code.StartsWith(datePrefix))
                .OrderByDescending(c => c.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastCustomer != null && !string.IsNullOrEmpty(lastCustomer.Code))
            {
                var lastCode = lastCustomer.Code;
                var numberPart = lastCode.Substring(datePrefix.Length);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{datePrefix}{nextNumber:D4}";
        }

        private async Task ValidateCustomerAsync(Customer customer, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState, bool isEdit = false)
        {
            // Validate phone number format
            if (!string.IsNullOrEmpty(customer.Phone))
            {
                var phoneRegex = new Regex(@"^[0-9+\-\s\(\)]{10,15}$");
                if (!phoneRegex.IsMatch(customer.Phone))
                {
                    modelState.AddModelError("Phone", "Số điện thoại không hợp lệ");
                }

                // Check phone uniqueness
                var existingPhone = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Phone == customer.Phone && 
                                           c.CustomerId != customer.CustomerId &&
                                           !c.IsArchived);
                if (existingPhone != null)
                {
                    modelState.AddModelError("Phone", "Số điện thoại đã tồn tại");
                }
            }

            // Validate email format and uniqueness
            if (!string.IsNullOrEmpty(customer.Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(customer.Email))
                {
                    modelState.AddModelError("Email", "Email không hợp lệ");
                }

                // Check email uniqueness
                var existingEmail = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == customer.Email && 
                                           c.CustomerId != customer.CustomerId &&
                                           !c.IsArchived);
                if (existingEmail != null)
                {
                    modelState.AddModelError("Email", "Email đã tồn tại");
                }
            }

            // Validate TaxCode uniqueness if provided
            if (!string.IsNullOrEmpty(customer.TaxCode))
            {
                var existingTaxCode = await _context.Customers
                    .FirstOrDefaultAsync(c => c.TaxCode == customer.TaxCode && 
                                           c.CustomerId != customer.CustomerId &&
                                           !c.IsArchived);
                if (existingTaxCode != null)
                {
                    modelState.AddModelError("TaxCode", "Mã số thuế đã tồn tại");
                }
            }
        }

        private bool CustomerExists(long id)
        {
            return _context.Customers.Any(e => e.CustomerId == id && !e.IsArchived);
        }
    }
}
