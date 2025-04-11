using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Product
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice, bool? lowStock)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            // Apply low stock filter
            if (lowStock.HasValue && lowStock.Value)
            {
                query = query.Where(p => p.QuantityInStock <= p.LowStockThreshold);
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.LowStock = lowStock;

            return View(await query.ToListAsync());
        }

        // AJAX endpoint for product search
        [AllowAnonymous]
        public async Task<IActionResult> Search(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice, bool? lowStock)
        {
            try
            {
                _logger.LogInformation($"AJAX search request: searchString={searchString}, categoryId={categoryId}, minPrice={minPrice}, maxPrice={maxPrice}, lowStock={lowStock}");
                
                var query = _context.Products
                    .Include(p => p.Category)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(p => p.Name.Contains(searchString));
                }

                // Apply category filter
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                // Apply price range filter
                if (minPrice.HasValue)
                {
                    if (minPrice.Value < 0)
                    {
                        _logger.LogWarning($"Invalid minPrice value: {minPrice}");
                        return Json(new { success = false, message = "Minimum price cannot be negative." });
                    }
                    query = query.Where(p => p.Price >= minPrice);
                }
                if (maxPrice.HasValue)
                {
                    if (maxPrice.Value < 0)
                    {
                        _logger.LogWarning($"Invalid maxPrice value: {maxPrice}");
                        return Json(new { success = false, message = "Maximum price cannot be negative." });
                    }
                    if (minPrice.HasValue && maxPrice.Value < minPrice.Value)
                    {
                        _logger.LogWarning($"Invalid price range: min={minPrice}, max={maxPrice}");
                        return Json(new { success = false, message = "Maximum price cannot be less than minimum price." });
                    }
                    query = query.Where(p => p.Price <= maxPrice);
                }

                // Apply low stock filter
                if (lowStock.HasValue && lowStock.Value)
                {
                    query = query.Where(p => p.QuantityInStock <= p.LowStockThreshold);
                }

                var products = await query.ToListAsync();
                _logger.LogInformation($"AJAX search returned {products.Count} products");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ProductList", products);
                }
                
                return Json(new { success = true, products = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AJAX product search");
                return Json(new { success = false, message = "An error occurred while searching for products. Please try again." });
            }
        }

        // GET: Product/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,QuantityInStock,LowStockThreshold,CategoryId")] Product product)
        {
            try
            {
                _logger.LogInformation($"Creating new product: {product.Name}");
                
                if (ModelState.IsValid)
                {
                    // Load the category
                    var category = await _context.Categories.FindAsync(product.CategoryId);
                    if (category != null)
                    {
                        product.Category = category;
                    }
                    
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Product created successfully: {product.Name}, ID: {product.Id}");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        // Return JSON response for AJAX requests
                        return Json(new { success = true, message = "Product created successfully.", productId = product.Id });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogWarning($"Invalid model state when creating product: {product.Name}");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning($"- {error.ErrorMessage}");
                    }
                }
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Return JSON response with validation errors for AJAX requests
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return Json(new { success = false, message = "Validation failed.", errors = errors });
                }
                
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating product: {product.Name}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while creating the product." });
                }
                
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,QuantityInStock,LowStockThreshold,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            try
            {
                _logger.LogInformation($"Updating product: {product.Name}, ID: {product.Id}");
                
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(product);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Product updated successfully: {product.Name}, ID: {product.Id}");
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            // Return JSON response for AJAX requests
                            return Json(new { success = true, message = "Product updated successfully.", productId = product.Id });
                        }
                        
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogError(ex, $"Concurrency error updating product: {product.Name}, ID: {product.Id}");
                        
                        if (!ProductExists(product.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                
                _logger.LogWarning($"Invalid model state when updating product: {product.Name}, ID: {product.Id}");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning($"- {error.ErrorMessage}");
                    }
                }
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Return JSON response with validation errors for AJAX requests
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return Json(new { success = false, message = "Validation failed.", errors = errors });
                }
                
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product: {product.Name}, ID: {product.Id}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while updating the product." });
                }
                
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting product with ID: {id}");
                
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found for deletion: ID {id}");
                    return NotFound();
                }
                
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product deleted successfully: {product.Name}, ID: {id}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Return JSON response for AJAX requests
                    return Json(new { success = true, message = "Product deleted successfully." });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID: {id}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while deleting the product." });
                }
                
                return RedirectToAction(nameof(Index));
            }
        }

        protected bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
} 