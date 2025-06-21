using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookies")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index(string? status, string? searchTerm, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Items)
                    .ThenInclude(i => i.OrderOptionItems)
                        .ThenInclude(oi => oi.OptionItem)
                .AsQueryable();

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status!.Code == status);
            }

            // 搜尋篩選
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int orderId))
                {
                    query = query.Where(o => o.Id == orderId);
                }
                else
                {
                    query = query.Where(o => o.Items.Any(i => i.Name.Contains(searchTerm)));
                }
            }

            // 日期篩選
            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= endDate.Value.AddDays(1));
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // 統計資料
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TodayOrders = await _context.Orders
                .Where(o => o.CreatedAt.Date == DateTime.Today)
                .CountAsync();
            ViewBag.PendingOrders = await _context.Orders
                .Include(o => o.Status)
                .Where(o => o.Status!.Code == "PENDING")
                .CountAsync();
            ViewBag.TodayRevenue = await _context.Orders
                .Where(o => o.CreatedAt.Date == DateTime.Today)
                .SumAsync(o => o.TotalAmount);

            // 狀態選項
            ViewBag.StatusOptions = await _context.OrderStatuses.ToListAsync();

            // 保留搜尋條件
            ViewBag.CurrentStatus = status;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Items)
                    .ThenInclude(i => i.OrderOptionItems)
                        .ThenInclude(oi => oi.OptionItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "找不到該訂單";
                return RedirectToAction("Index");
            }

            // 取得可用的狀態選項
            ViewBag.StatusOptions = await _context.OrderStatuses.ToListAsync();

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int statusId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"收到更新狀態請求：OrderId={id}, StatusId={statusId}");

                var order = await _context.Orders
                    .Include(o => o.Status)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    System.Diagnostics.Debug.WriteLine($"找不到訂單 ID: {id}");
                    return Json(new { success = false, message = "找不到該訂單" });
                }

                var newStatus = await _context.OrderStatuses
                    .FirstOrDefaultAsync(s => s.Id == statusId);

                if (newStatus == null)
                {
                    System.Diagnostics.Debug.WriteLine($"找不到狀態 ID: {statusId}");
                    return Json(new { success = false, message = "無效的狀態" });
                }

                // 記錄更新前的狀態
                var oldStatusName = order.Status?.Name ?? "未知";

                // 更新狀態
                order.OrderStatusId = statusId;
                var rowsAffected = await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"更新完成，影響行數: {rowsAffected}");
                System.Diagnostics.Debug.WriteLine($"狀態從 {oldStatusName} 更新為 {newStatus.Name}");

                if (rowsAffected > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"訂單 #{order.Id} 狀態已從「{oldStatusName}」更新為「{newStatus.Name}」",
                        newStatus = newStatus.Name,
                        newStatusCode = newStatus.Code
                    });
                }
                else
                {
                    return Json(new { success = false, message = "更新失敗，沒有任何變更" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新狀態錯誤: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"詳細錯誤: {ex}");

                return Json(new { success = false, message = $"系統錯誤：{ex.Message}" });
            }
        }
        // POST: Admin/Order/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.OrderOptionItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["Error"] = "找不到該訂單";
                    return RedirectToAction("Index");
                }

                // 刪除相關的子項目
                foreach (var item in order.Items)
                {
                    _context.OrderOptionItems.RemoveRange(item.OrderOptionItems);
                }
                _context.OrderItems.RemoveRange(order.Items);
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"訂單 #{order.Id} 已成功刪除";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"刪除訂單錯誤: {ex.Message}");
                TempData["Error"] = "刪除訂單時發生錯誤";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Order/Statistics
        public async Task<IActionResult> Statistics()
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var stats = new
            {
                TodayOrders = await _context.Orders.Where(o => o.CreatedAt.Date == today).CountAsync(),
                TodayRevenue = await _context.Orders.Where(o => o.CreatedAt.Date == today).SumAsync(o => o.TotalAmount),
                WeekOrders = await _context.Orders.Where(o => o.CreatedAt >= thisWeek).CountAsync(),
                WeekRevenue = await _context.Orders.Where(o => o.CreatedAt >= thisWeek).SumAsync(o => o.TotalAmount),
                MonthOrders = await _context.Orders.Where(o => o.CreatedAt >= thisMonth).CountAsync(),
                MonthRevenue = await _context.Orders.Where(o => o.CreatedAt >= thisMonth).SumAsync(o => o.TotalAmount),

                // 狀態統計
                StatusStats = await _context.Orders
                    .Include(o => o.Status)
                    .GroupBy(o => o.Status!.Name)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(),

                // 熱門商品
                PopularItems = await _context.OrderItems
                    .GroupBy(oi => oi.Name)
                    .Select(g => new { ItemName = g.Key, Quantity = g.Sum(x => x.Quantity) })
                    .OrderByDescending(x => x.Quantity)
                    .Take(10)
                    .ToListAsync()
            };

            return Json(stats);
        }
    }
}