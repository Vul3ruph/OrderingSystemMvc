// Areas/Admin/Controllers/DashboardController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;
using OrderingSystemMvc.ViewModels;
using System.Security.Claims;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookies")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 取得當前管理員資訊
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // 統計資料
            var dashboardData = new DashboardVM
            {
                // 基本統計
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalMenuItems = await _context.MenuItems.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),

                // 今日統計
                TodayOrders = await _context.Orders
                    .Where(o => o.CreatedAt.Date == DateTime.Today)
                    .CountAsync(),

                TodayRevenue = await _context.Orders
                    .Where(o => o.CreatedAt.Date == DateTime.Today)
                    .SumAsync(o => o.TotalAmount),

                // 本週統計
                WeeklyOrders = await _context.Orders
                    .Where(o => o.CreatedAt >= DateTime.Today.AddDays(-7))
                    .CountAsync(),

                WeeklyRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= DateTime.Today.AddDays(-7))
                    .SumAsync(o => o.TotalAmount),

                // 本月統計
                MonthlyOrders = await _context.Orders
                    .Where(o => o.CreatedAt.Month == DateTime.Now.Month && o.CreatedAt.Year == DateTime.Now.Year)
                    .CountAsync(),

                MonthlyRevenue = await _context.Orders
                    .Where(o => o.CreatedAt.Month == DateTime.Now.Month && o.CreatedAt.Year == DateTime.Now.Year)
                    .SumAsync(o => o.TotalAmount),

                // 最近訂單
                RecentOrders = await _context.Orders
                    .Include(o => o.Status)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new RecentOrderViewModel
                    {
                        Id = o.Id,
                        CreatedAt = o.CreatedAt,
                        TotalAmount = o.TotalAmount,
                        StatusName = o.Status.Name,
                        StatusColor = o.Status.ColorClass,
                        CustomerEmail = _userManager.Users
                            .Where(u => u.Id == o.UserId)
                            .Select(u => u.Email)
                            .FirstOrDefault()
                    })
                    .ToListAsync(),

                // 熱門菜品
                PopularMenuItems = await _context.OrderItems
                    .GroupBy(oi => oi.MenuItemId)
                    .Select(g => new PopularMenuItemViewModel
                    {
                        MenuItemId = g.Key,
                        Name = _context.MenuItems.Where(mi => mi.Id == g.Key).Select(mi => mi.Name).FirstOrDefault(),
                        TotalQuantity = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToListAsync(),

                // 訂單狀態統計
                OrderStatusStats = await _context.Orders
                    .Include(o => o.Status)
                    .GroupBy(o => o.Status)
                    .Select(g => new OrderStatusStatViewModel
                    {
                        StatusName = g.Key.Name,
                        StatusColor = g.Key.ColorClass,
                        Count = g.Count(),
                        Percentage = 0 // 會在後面計算
                    })
                    .ToListAsync(),

                // 當前管理員資訊
                CurrentAdmin = new AdminInfoViewModel
                {
                    DisplayName = currentUser?.DisplayName ?? "管理員",
                    Email = currentUser?.Email ?? "",
                    UserType = User.FindFirst("UserType")?.Value ?? "",
                    LastLoginAt = currentUser?.LastLoginAt
                }
            };

            // 計算訂單狀態百分比
            var totalOrdersForPercent = dashboardData.OrderStatusStats.Sum(x => x.Count);
            if (totalOrdersForPercent > 0)
            {
                foreach (var stat in dashboardData.OrderStatusStats)
                {
                    stat.Percentage = (double)stat.Count / totalOrdersForPercent * 100;
                }
            }

            return View(dashboardData);
        }
    }

    
}