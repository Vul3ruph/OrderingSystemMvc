using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Helpers;
using OrderingSystemMvc.Models;
namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("User")]

    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var categories = await _context.Categories.OrderBy(c => c.SortOrder).ToListAsync();

            var itemsQuery = _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.MenuItemOptions)
                    .ThenInclude(mio => mio.Option)
                        .ThenInclude(o => o.OptionItems)  // 🔧 正確的深層 Include
                .Where(m => m.IsAvailable);

            if (categoryId.HasValue)
            {
                itemsQuery = itemsQuery.Where(m => m.CategoryId == categoryId.Value);
            }

            var items = await itemsQuery.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            return View(items);
        }

            [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
                return NotFound();

            var cartItem = new CartItem
            {
                MenuItemId = item.Id,
                Name = item.Name,
                Price = item.Price,
                ImageUrl = item.ImageUrl
            };

            CartHelper.AddToCart(HttpContext, cartItem);

            TempData["Success"] = $"已將「{item.Name}」加入購物車";
            return RedirectToAction("Index", new { categoryId = item.CategoryId });
        }
    }
}