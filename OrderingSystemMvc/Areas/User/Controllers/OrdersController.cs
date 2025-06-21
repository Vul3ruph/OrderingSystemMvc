using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;
using System.Security.Claims;

namespace OrderingSystemMvc.Areas.User.Controllers
{
    [Area("User")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: 訂單列表
        public async Task<IActionResult> Index()
        {
            var orders = new List<Order>();

            if (User.Identity?.IsAuthenticated == true)
            {
                // 已登入用戶：顯示該用戶的所有訂單
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                orders = await _context.Orders
                    .Include(o => o.Status)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.OrderOptionItems)
                            .ThenInclude(oi => oi.OptionItem)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                // 未登入用戶：提示登入
                ViewBag.IsLoggedIn = false;
                return View(new List<Order>());
            }

            ViewBag.IsLoggedIn = true;
            return View(orders);
        }

        // GET: 訂單詳細資訊
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

            // 檢查權限：只有訂單擁有者或管理員可以查看
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId != userId)
                {
                    TempData["Error"] = "您沒有權限查看此訂單";
                    return RedirectToAction("Index");
                }

                // 取得用戶電話號碼
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserPhone = user?.PhoneNumber;
            }
            else
            {
                // 未登入用戶無法查看訂單詳情
                TempData["Error"] = "請先登入以查看訂單詳情";
                return RedirectToAction("Login", "Account");
            }

            return View(order);
        }

        // GET: 取消訂單確認頁面
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Status)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "找不到該訂單";
                return RedirectToAction("Index");
            }

            // 檢查是否可以取消
            if (order.Status?.Code != "PENDING")
            {
                TempData["Error"] = "此訂單已無法取消";
                return RedirectToAction("Details", new { id });
            }

            // 檢查權限
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId != userId)
                {
                    TempData["Error"] = "您沒有權限操作此訂單";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "請先登入";
                return RedirectToAction("Login", "Account");
            }

            return View(order);
        }

        // POST: 確認取消訂單
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Status)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["Error"] = "找不到該訂單";
                    return RedirectToAction("Index");
                }

                // 檢查權限
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (order.UserId != userId)
                    {
                        TempData["Error"] = "您沒有權限操作此訂單";
                        return RedirectToAction("Index");
                    }
                }

                // 檢查是否可以取消
                if (order.Status?.Code != "PENDING")
                {
                    TempData["Error"] = "此訂單已無法取消";
                    return RedirectToAction("Details", new { id });
                }

                // 更新訂單狀態為已取消
                var cancelledStatus = await _context.OrderStatuses
                    .FirstOrDefaultAsync(s => s.Code == "CANCELLED");

                if (cancelledStatus != null)
                {
                    order.OrderStatusId = cancelledStatus.Id;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "訂單已成功取消";
                }
                else
                {
                    TempData["Error"] = "系統錯誤，無法取消訂單";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"取消訂單錯誤: {ex.Message}");
                TempData["Error"] = "取消訂單時發生錯誤，請稍後再試";
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: 訂單搜尋
        public async Task<IActionResult> Search(string searchTerm, DateTime? startDate, DateTime? endDate)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var query = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Items)
                .Where(o => o.UserId == userId);

            // 搜尋條件
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

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.IsLoggedIn = true;

            return View("Index", orders);
        }
    }
}