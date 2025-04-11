namespace WebApplication1.Models
{
    public class HomeViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }
} 