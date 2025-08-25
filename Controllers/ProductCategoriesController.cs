using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ChainStoreSalesManagement.Controllers
{
    [Authorize]
    public class ProductCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductCategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ProductCategories
        public async Task<IActionResult> Index(string searchTerm, int page = 1, int pageSize = 8)
        {
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            
            var query = _context.ProductCategories
                .AsQueryable();

            // Search by Code or Name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var categories = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pagination info
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPages;
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalCount);

            return View(categories);
        }

        // GET: ProductCategories/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
                
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // GET: ProductCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,IsActive")] ProductCategory productCategory, IFormFile? categoryImage)
        {
            // Validate unique Name
            var existingCategory = await _context.ProductCategories
                .Where(p => p.Name == productCategory.Name)
                .FirstOrDefaultAsync();

            if (existingCategory != null)
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (categoryImage != null && categoryImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + categoryImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await categoryImage.CopyToAsync(fileStream);
                    }
                    
                    productCategory.ImagePath = "/images/categories/" + uniqueFileName;
                }

                // Set required fields
                productCategory.CreatedAt = DateTime.UtcNow;
                
                _context.Add(productCategory);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(productCategory);
        }

        // GET: ProductCategories/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // POST: ProductCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("CategoryId,Name,Description,IsActive,CreatedAt,ImagePath")] ProductCategory productCategory, IFormFile? categoryImage, bool removeImage = false)
        {
            if (id != productCategory.CategoryId)
            {
                return NotFound();
            }

            // Validate unique Name excluding current record
            var existingCategory = await _context.ProductCategories
                .Where(p => p.Name == productCategory.Name && p.CategoryId != id)
                .FirstOrDefaultAsync();

            if (existingCategory != null)
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current category to preserve existing image path
                    var currentCategory = await _context.ProductCategories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryId == id);
                    if (currentCategory == null)
                    {
                        return NotFound();
                    }

                    // Handle image removal
                    if (removeImage)
                    {
                        // Delete old image file if it exists
                        if (!string.IsNullOrEmpty(currentCategory.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, currentCategory.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        productCategory.ImagePath = null;
                    }
                    // Handle new image upload
                    else if (categoryImage != null && categoryImage.Length > 0)
                    {
                        // Delete old image file if it exists
                        if (!string.IsNullOrEmpty(currentCategory.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, currentCategory.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + categoryImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await categoryImage.CopyToAsync(fileStream);
                        }
                        
                        productCategory.ImagePath = "/images/categories/" + uniqueFileName;
                    }
                    // Keep existing image
                    else
                    {
                        productCategory.ImagePath = currentCategory.ImagePath;
                    }

                    productCategory.ModifiedAt = DateTime.UtcNow;
                    _context.Update(productCategory);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductCategoryExists(productCategory.CategoryId))
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

            return View(productCategory);
        }

        // GET: ProductCategories/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
                
            if (productCategory == null)
            {
                return NotFound();
            }

            // Check if there are any products referencing this category
            var productCount = await _context.Products
                .CountAsync(p => p.CategoryId == id);
                
            ViewBag.ProductCount = productCount;
            ViewBag.CanDelete = productCount == 0;

            return View(productCategory);
        }

        // POST: ProductCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            // Check if there are any products referencing this category
            var productCount = await _context.Products
                .CountAsync(p => p.CategoryId == id);
                
            if (productCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa danh mục này vì có {productCount} sản phẩm đang sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory != null)
            {
                _context.ProductCategories.Remove(productCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductCategoryExists(long id)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == id);
        }
    }
}
