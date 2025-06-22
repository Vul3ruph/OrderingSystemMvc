using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Helpers;
using OrderingSystemMvc.Models;
using System.Security.Claims;

namespace OrderingSystemMvc.Areas.User.Controllers
{
    [Area("User")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext);
            ViewBag.Total = cart.Sum(c => (c.Price + c.ExtraTotal) * c.Quantity);
            ViewBag.IsLoggedIn = User.Identity?.IsAuthenticated ?? false;
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int menuItemId, List<int>? selectedOptionItemIds)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"接收到餐點 ID: {menuItemId}");
                System.Diagnostics.Debug.WriteLine($"選項 IDs: {string.Join(", ", selectedOptionItemIds ?? new List<int>())}");

                var item = _context.MenuItems.FirstOrDefault(x => x.Id == menuItemId);
                if (item == null)
                {
                    TempData["Error"] = "找不到該餐點";
                    return RedirectToAction("Index");
                }

                var sortedOptionIds = selectedOptionItemIds?.OrderBy(id => id).ToList() ?? new();

                var optionSummary = string.Join("、",
                    sortedOptionIds
                    .Where(id => StaticData.OptionItemDict.ContainsKey(id))
                    .Select(id => StaticData.OptionItemDict[id].Name));

                var cartItem = new CartItem
                {
                    MenuItemId = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = 1,
                    ImageUrl = item.ImageUrl,
                    SelectedOptionItemIds = sortedOptionIds,
                    OptionSummary = optionSummary
                };

                CartHelper.AddToCart(HttpContext, cartItem);
                var cartCount = CartHelper.GetCartCount(HttpContext);

                System.Diagnostics.Debug.WriteLine($"購物車數量: {cartCount}");

                // 檢查是否來自購物車頁面
                string? referer = Request.Headers["Referer"].ToString();
                bool isFromCart = !string.IsNullOrEmpty(referer) && referer.Contains("/Cart");

                // 如果是 AJAX 請求，返回 JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        success = true,
                        message = "已加入購物車",
                        cartCount = cartCount
                    });
                }

                // 如果來自購物車頁面，回到購物車；否則回到選單
                if (isFromCart)
                {
                    TempData["Toast"] = "商品數量已增加";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Toast"] = "已加入購物車";
                    return RedirectToAction("Index", "Menu");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加入購物車錯誤: {ex.Message}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "系統錯誤，請稍後再試" });
                }

                TempData["Error"] = "操作失敗，請稍後再試";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Subtract(int id)
        {
            try
            {
                var cart = CartHelper.GetCart(HttpContext);
                var item = cart.FirstOrDefault(c => c.MenuItemId == id);

                if (item != null)
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                    {
                        cart.Remove(item);
                        TempData["Toast"] = "商品已從購物車移除";
                    }
                    else
                    {
                        TempData["Toast"] = "商品數量已減少";
                    }

                    CartHelper.SaveCart(HttpContext, cart);
                }
                else
                {
                    TempData["Error"] = "找不到該商品";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"減少數量錯誤: {ex.Message}");
                TempData["Error"] = "操作失敗，請稍後再試";
            }

            // 永遠回到購物車頁面
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            try
            {
                var cart = CartHelper.GetCart(HttpContext);
                var item = cart.FirstOrDefault(c => c.MenuItemId == id);

                if (item != null)
                {
                    cart.Remove(item);
                    CartHelper.SaveCart(HttpContext, cart);
                    TempData["Toast"] = "商品已從購物車移除";
                }
                else
                {
                    TempData["Error"] = "找不到該商品";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"移除商品錯誤: {ex.Message}");
                TempData["Error"] = "操作失敗，請稍後再試";
            }

            // 永遠回到購物車頁面
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cart = CartHelper.GetCart(HttpContext);
                if (!cart.Any())
                {
                    TempData["Error"] = "購物車是空的";
                    return RedirectToAction("Index");
                }
                // 檢查用戶是否已登入
                if (!User.Identity?.IsAuthenticated == true)
                    {
                    // 儲存當前的購物車到 TempData，登入後可以恢復
                    TempData["PendingCart"] = System.Text.Json.JsonSerializer.Serialize(cart);

                    // 儲存返回的 URL，登入後直接回到結帳頁面
                    TempData["ReturnUrl"] = Url.Action("Index", "Cart");

                    // 設置提示訊息
                    TempData["Info"] = "請先登入以完成結帳";

                    return RedirectToAction("Login", "Account");
                }
                // 確保有預設的 OrderStatus
                var defaultStatus = _context.OrderStatuses.FirstOrDefault(s => s.Code == "PENDING");
                if (defaultStatus == null)
                {
                    defaultStatus = new OrderStatus
                    {
                        Code = "PENDING",
                        Name = "待處理",
                        ColorClass = "bg-warning"
                    };
                    _context.OrderStatuses.Add(defaultStatus);
                    _context.SaveChanges();
                }
                

                // 取得當前用戶 ID（如果已登入）
                string? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                var order = new Order
                {
                    CreatedAt = DateTime.Now,
                    TotalAmount = cart.Sum(c => (c.Price + c.ExtraTotal) * c.Quantity),
                    UserId = userId, // 如果未登入則為 null
                    OrderStatusId = defaultStatus.Id,
                    Items = cart.Select(c => new OrderItem
                    {
                        MenuItemId = c.MenuItemId,
                        Name = c.Name,
                        Price = c.Price,
                        Quantity = c.Quantity,
                        OrderOptionItems = c.SelectedOptionItemIds.Select(optionId => new OrderOptionItem
                        {
                            OptionItemId = optionId,
                            ExtraPrice = StaticData.OptionItemDict.ContainsKey(optionId)
                                ? StaticData.OptionItemDict[optionId].ExtraPrice
                                : 0
                        }).ToList()
                    }).ToList()
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // 清空購物車
                CartHelper.SaveCart(HttpContext, new List<CartItem>());

                var successMessage = User.Identity?.IsAuthenticated == true
                    ? $"✅ 訂單已送出！訂單編號：{order.Id}，感謝您的訂購！"
                    : $"✅ 訂單已送出！訂單編號：{order.Id}，建議您註冊會員以便查詢訂單狀態。";

                TempData["Toast"] = successMessage;
                return RedirectToAction("Success", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"結帳錯誤: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"內部錯誤: {ex.InnerException?.Message}");

                TempData["Error"] = "結帳時發生錯誤，請稍後再試";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Success(int? orderId)
        {
            ViewBag.OrderId = orderId;
            ViewBag.IsLoggedIn = User.Identity?.IsAuthenticated ?? false;
            return View(orderId);
        }
    }
}