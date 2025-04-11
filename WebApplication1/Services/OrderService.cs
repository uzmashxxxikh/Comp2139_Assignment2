using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<List<Order>> GetOrdersByEmailAsync(string email);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
        Task<List<Order>> GetAllOrdersAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                // Validate stock levels first
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");
                    }
                    if (product.QuantityInStock < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.QuantityInStock}, Requested: {item.Quantity}");
                    }
                }

                // Ensure DateTime is in UTC
                order.OrderDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                order.TotalAmount = 0;

                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        item.UnitPrice = product.Price;
                        order.TotalAmount += item.UnitPrice * item.Quantity;

                        // Update product quantity
                        product.QuantityInStock -= item.Quantity;
                    }
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Order created successfully with ID: {order.Id}");
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating order: {ex.Message}");
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetOrdersByEmailAsync(string email)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.GuestEmail == email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order updated successfully with ID: {order.Id}");
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency error updating order: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (order != null)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Order deleted successfully with ID: {id}");
                    return true;
                }
                
                _logger.LogWarning($"Order not found with ID: {id}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
} 