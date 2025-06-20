using Microsoft.AspNetCore.Mvc;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Helpers;
using OrderingSystemMvc.Models;
namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("User")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext);
            ViewBag.Total = cart.Sum(c => c.Price * c.Quantity);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int menuItemId)
        {
            var item = _context.MenuItems.FirstOrDefault(x => x.Id == menuItemId);
            if (item == null) return NotFound();

            var cartItem = new CartItem
            {
                MenuItemId = item.Id,
                Name = item.Name,
                Price = item.Price,
                Quantity = 1,
                ImageUrl = item.ImageUrl
            };

            // ✅ 加入購物車
            CartHelper.AddToCart(HttpContext, cartItem);

            // ✅ 取得購物車的總數量
            int totalQuantity = CartHelper.GetCartCount(HttpContext);

            return Json(new
            {
                success = true,
                message = "✅ 已加入購物車",
                cartCount = totalQuantity  // 👈 傳回前端
            });
        }




        [HttpPost]
        public IActionResult Subtract(int id)
        {
            var cart = CartHelper.GetCart(HttpContext);
            var item = cart.FirstOrDefault(c => c.MenuItemId == id);
            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    cart.Remove(item);
            }
            CartHelper.SaveCart(HttpContext, cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = CartHelper.GetCart(HttpContext);
            var item = cart.FirstOrDefault(c => c.MenuItemId == id);
            if (item != null) cart.Remove(item);
            CartHelper.SaveCart(HttpContext, cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            var cart = CartHelper.GetCart(HttpContext);
            if (!cart.Any()) return RedirectToAction("Index");

            var order = new Order
            {
                TotalAmount = cart.Sum(c => c.Price * c.Quantity),
                Items = cart.Select(c => new OrderItem
                {
                    MenuItemId = c.MenuItemId,
                    Name = c.Name,
                    Price = c.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();
            CartHelper.SaveCart(HttpContext, new List<CartItem>()); // 清空

            TempData["Toast"] = "✅ 訂單已送出，感謝您的訂購";

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }

}