using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Infrastructure.Persistence;
using ChainStoreSalesManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChainStoreSalesManagement.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index(long? shopId, long? categoryId, int? brandId, bool? canSell, string keyword = "", int page = 1, int pageSize = 8)
        {
            var query = _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // Filter by shop
            if (shopId.HasValue)
            {
                query = query.Where(p => p.ShopId == shopId.Value);
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by brand
            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Filter by CanSell
            if (canSell.HasValue)
            {
                query = query.Where(p => p.CanSell == canSell.Value);
            }

            // Search by keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) || (p.Code != null && p.Code.Contains(keyword)) || (p.Description != null && p.Description.Contains(keyword)));
            }

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Setup ViewBag for pagination and filters
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalItems);
            ViewBag.TotalCount = totalItems;
            
            // Current filters
            ViewBag.ShopId = shopId;
            ViewBag.CategoryId = categoryId;
            ViewBag.BrandId = brandId;
            ViewBag.CanSell = canSell;
            ViewBag.Keyword = keyword;

            // Filter options
            ViewBag.Shops = new SelectList(await _context.Shops.OrderBy(s => s.Name).ToListAsync(), "ShopId", "Name", shopId);
            ViewBag.Categories = new SelectList(await _context.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "CategoryId", "Name", categoryId);
            ViewBag.Brands = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "BrandId", "Name", brandId);

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.ProductId == id && !m.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create(long? shopId)
        {
            ViewBag.ShopId = new SelectList(await _context.Shops.OrderBy(s => s.Name).ToListAsync(), "ShopId", "Name", shopId);
            ViewBag.CategoryId = new SelectList(await _context.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "CategoryId", "Name");
            ViewBag.BrandId = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "BrandId", "Name");

            var product = new Product();
            if (shopId.HasValue)
            {
                product.ShopId = shopId.Value;
            }

            return View(product);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShopId,CategoryId,Code,Name,BrandId,CanSell,Unit,Description,Price,Sku,Barcode,OptionSummary,CostPrice,TrackSerial")] Product product, List<IFormFile>? imageFiles, List<string>? imageUrls)
        {
            // Validate required fields
            if (product.ShopId <= 0)
            {
                ModelState.AddModelError("ShopId", "Vui lòng chọn cửa hàng");
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên sản phẩm");
            }

            if (string.IsNullOrWhiteSpace(product.Sku))
            {
                ModelState.AddModelError("Sku", "Vui lòng nhập mã SKU");
            }

            // Validate unique code per shop
            if (!string.IsNullOrEmpty(product.Code) && await _context.Products.AnyAsync(p => p.Code == product.Code && p.ShopId == product.ShopId && !p.IsDeleted))
            {
                ModelState.AddModelError("Code", "Mã sản phẩm đã tồn tại trong cửa hàng này.");
            }

            // Validate unique SKU
            if (!string.IsNullOrEmpty(product.Sku) && await _context.Products.AnyAsync(p => p.Sku == product.Sku && !p.IsDeleted))
            {
                ModelState.AddModelError("Sku", "Mã SKU đã tồn tại.");
            }

            // Validate unique barcode if provided
            if (!string.IsNullOrEmpty(product.Barcode) && await _context.Products.AnyAsync(p => p.Barcode == product.Barcode && !p.IsDeleted))
            {
                ModelState.AddModelError("Barcode", "Mã vạch đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.CreatedAt = DateTime.UtcNow;
                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    // Handle multiple image uploads
                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        foreach (var imageFile in imageFiles)
                        {
                            if (imageFile != null && imageFile.Length > 0)
                            {
                                await SaveProductImage(product.ProductId, imageFile);
                            }
                        }
                    }
                    
                    // Handle URL images
                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        foreach (var imageUrl in imageUrls)
                        {
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                await SaveProductImageFromUrl(product.ProductId, imageUrl);
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(Index), new { shopId = product.ShopId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi lưu sản phẩm: {ex.Message}");
                }
            }

            // Reload dropdowns
            ViewBag.ShopId = new SelectList(await _context.Shops.OrderBy(s => s.Name).ToListAsync(), "ShopId", "Name", product.ShopId);
            ViewBag.CategoryId = new SelectList(await _context.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "CategoryId", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "BrandId", "Name", product.BrandId);

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ShopId = new SelectList(await _context.Shops.OrderBy(s => s.Name).ToListAsync(), "ShopId", "Name", product.ShopId);
            ViewBag.CategoryId = new SelectList(await _context.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "CategoryId", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "BrandId", "Name", product.BrandId);

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ProductId,ShopId,CategoryId,Code,Name,BrandId,CanSell,Unit,Description,Price,Sku,Barcode,OptionSummary,CostPrice,TrackSerial,CreatedAt,RowVersion")] Product product, List<IFormFile>? imageFiles, List<string>? imageUrls, string? deletedImageIds)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            // Validate required fields
            if (product.ShopId <= 0)
            {
                ModelState.AddModelError("ShopId", "Vui lòng chọn cửa hàng");
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên sản phẩm");
            }

            if (string.IsNullOrWhiteSpace(product.Sku))
            {
                ModelState.AddModelError("Sku", "Vui lòng nhập mã SKU");
            }

            // Validate unique code per shop (exclude current product)
            if (!string.IsNullOrEmpty(product.Code) && await _context.Products.AnyAsync(p => p.Code == product.Code && p.ShopId == product.ShopId && p.ProductId != product.ProductId && !p.IsDeleted))
            {
                ModelState.AddModelError("Code", "Mã sản phẩm đã tồn tại trong cửa hàng này.");
            }
            
            // Validate unique SKU (exclude current product)
            if (!string.IsNullOrEmpty(product.Sku) && await _context.Products.AnyAsync(p => p.Sku == product.Sku && p.ProductId != product.ProductId && !p.IsDeleted))
            {
                ModelState.AddModelError("Sku", "Mã SKU đã tồn tại.");
            }

            // Validate unique barcode if provided (exclude current product)
            if (!string.IsNullOrEmpty(product.Barcode) && await _context.Products.AnyAsync(p => p.Barcode == product.Barcode && p.ProductId != product.ProductId && !p.IsDeleted))
            {
                ModelState.AddModelError("Barcode", "Mã vạch đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.ModifiedAt = DateTime.UtcNow;
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // Handle deleted images
                    if (!string.IsNullOrEmpty(deletedImageIds))
                    {
                        var imageIdsToDelete = deletedImageIds.Split(',')
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .Select(id => Convert.ToInt64(id.Trim()))
                            .ToList();

                        foreach (var imageId in imageIdsToDelete)
                        {
                            var imageToDelete = await _context.ProductImages.FindAsync(imageId);
                            if (imageToDelete != null && imageToDelete.ProductId == product.ProductId)
                            {
                                imageToDelete.IsDeleted = true;
                                // Optionally delete the physical file
                                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, imageToDelete.FilePath.TrimStart('/'));
                                if (System.IO.File.Exists(physicalPath))
                                {
                                    System.IO.File.Delete(physicalPath);
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Handle new image uploads
                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        foreach (var imageFile in imageFiles)
                        {
                            if (imageFile != null && imageFile.Length > 0)
                            {
                                await SaveProductImage(product.ProductId, imageFile);
                            }
                        }
                    }
                    
                    // Handle URL images
                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        foreach (var imageUrl in imageUrls)
                        {
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                await SaveProductImageFromUrl(product.ProductId, imageUrl);
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { shopId = product.ShopId });
            }

            // Reload dropdowns
            ViewBag.ShopId = new SelectList(await _context.Shops.OrderBy(s => s.Name).ToListAsync(), "ShopId", "Name", product.ShopId);
            ViewBag.CategoryId = new SelectList(await _context.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "CategoryId", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "BrandId", "Name", product.BrandId);

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                product.ModifiedAt = DateTime.UtcNow;
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Products/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null && product.IsDeleted)
            {
                product.IsDeleted = false;
                product.ModifiedAt = DateTime.UtcNow;
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Khôi phục sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.ProductId == id && !e.IsDeleted);
        }

        private async Task SaveProductImage(long productId, IFormFile imageFile)
        {
            // Validate file type and size
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            
            if (!allowedTypes.Contains(fileExtension))
            {
                throw new InvalidOperationException("Chỉ chấp nhận file ảnh định dạng .jpg, .jpeg, .png");
            }

            if (imageFile.Length > 1024 * 1024) // 1MB
            {
                throw new InvalidOperationException("Kích thước file không được vượt quá 1MB");
            }

            // Create upload directory
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"{productId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Save to database
            var productImage = new ProductImage
            {
                ProductId = productId,
                FilePath = $"/uploads/products/{fileName}",
                IsPrimary = !await _context.ProductImages.AnyAsync(pi => pi.ProductId == productId)
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
        }
        
        private async Task SaveProductImageFromUrl(long productId, string imageUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType;
                
                // Determine file extension from content type
                string fileExtension = contentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/jpg" => ".jpg", 
                    "image/png" => ".png",
                    _ => Path.GetExtension(new Uri(imageUrl).AbsolutePath).ToLower()
                };
                
                if (string.IsNullOrEmpty(fileExtension) || !new[] { ".jpg", ".jpeg", ".png" }.Contains(fileExtension))
                {
                    throw new InvalidOperationException("URL không phải là hình ảnh hợp lệ");
                }

                // Create upload directory
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileName = $"{productId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                // Save to database
                var productImage = new ProductImage
                {
                    ProductId = productId,
                    FilePath = $"/uploads/products/{fileName}",
                    IsPrimary = !await _context.ProductImages.AnyAsync(pi => pi.ProductId == productId)
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Ignore URL image errors to not break the product creation
                // Could log the error here if needed
            }
        }
    }
}
