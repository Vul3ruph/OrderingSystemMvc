namespace OrderingSystemMvc.ViewModels
{
    public class DashboardVM
    {    
            public int TotalUsers { get; set; }
            public int TotalOrders { get; set; }
            public int TotalMenuItems { get; set; }
            public int TotalCategories { get; set; }

            public int TodayOrders { get; set; }
            public decimal TodayRevenue { get; set; }

            public int WeeklyOrders { get; set; }
            public decimal WeeklyRevenue { get; set; }

            public int MonthlyOrders { get; set; }
            public decimal MonthlyRevenue { get; set; }

            public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
            public List<PopularMenuItemViewModel> PopularMenuItems { get; set; } = new();
            public List<OrderStatusStatViewModel> OrderStatusStats { get; set; } = new();
            public AdminInfoViewModel CurrentAdmin { get; set; } = new();
        }

        public class RecentOrderViewModel
        {
            public int Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public decimal TotalAmount { get; set; }
            public string StatusName { get; set; }
            public string StatusColor { get; set; }
            public string CustomerEmail { get; set; }
        }

        public class PopularMenuItemViewModel
        {
            public int MenuItemId { get; set; }
            public string Name { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        public class OrderStatusStatViewModel
        {
            public string StatusName { get; set; }
            public string StatusColor { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public class AdminInfoViewModel
        {
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public string UserType { get; set; }
            public DateTime? LastLoginAt { get; set; }
        
    }
}
