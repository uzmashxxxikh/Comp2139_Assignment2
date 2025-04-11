using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, IOrderService orderService, ILogger<OrderController> logger)
        {
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        // GET: Order
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync());
        }

        // GET: Order/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("GuestName,GuestEmail,OrderItems")] Order order)
        {
            try
            {
                // Set default values
                order.OrderDate = DateTime.Now;
                order.TotalAmount = 0;

                // Validate order items
                if (order.OrderItems == null || !order.OrderItems.Any())
                {
                    ModelState.AddModelError("", "At least one order item is required.");
                }
                else
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Quantity <= 0)
                        {
                            ModelState.AddModelError("", "Quantity must be greater than 0.");
                            break;
                        }

                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null)
                        {
                            ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
                            break;
                        }

                        if (product.QuantityInStock < item.Quantity)
                        {
                            ModelState.AddModelError("", $"Insufficient stock for product {product.Name}. Available: {product.QuantityInStock}");
                            break;
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var createdOrder = await _orderService.CreateOrderAsync(order);
                    _logger.LogInformation($"Order created successfully with ID: {createdOrder.Id}");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, orderId = createdOrder.Id });
                    }

                    TempData["SuccessMessage"] = "Order created successfully!";
                    return RedirectToAction(nameof(Track), new { id = createdOrder.Id });
                }

                _logger.LogWarning("Invalid model state when creating order: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while creating the order." });
                }

                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }
        }

        // GET: Order/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Products = _context.Products.ToList();
            return View(order);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GuestName,GuestEmail,OrderDate,TotalAmount,OrderItems")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewBag.Products = _context.Products.ToList();
            return View(order);
        }

        // GET: Order/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        // GET: Order/Track
        [AllowAnonymous]
        public IActionResult Track()
        {
            return View();
        }

        // GET: Order/Track/5
        [AllowAnonymous]
        public async Task<IActionResult> TrackById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View("Track", order);
        }

        // GET: Order/TrackByEmail
        [AllowAnonymous]
        public IActionResult TrackByEmail()
        {
            return View();
        }

        // POST: Order/TrackByEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> TrackByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Please enter your email address.");
                return View();
            }

            var orders = await _orderService.GetOrdersByEmailAsync(email);
            if (!orders.Any())
            {
                ModelState.AddModelError("", "No orders found for this email address.");
                return View();
            }

            return View("TrackList", orders);
        }

        // POST: Order/DeleteOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Order deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Order not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                TempData["ErrorMessage"] = "An error occurred while deleting the order.";
            }

            return RedirectToAction(nameof(Track));
        }
    }
} 