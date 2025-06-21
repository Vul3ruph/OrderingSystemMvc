using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
            return View(categories);
        }

        // GET: Create or Edit
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                ViewData["Title"] = "新增分類";
                return View(new Category());
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            ViewData["Title"] = "編輯分類";
            return View(category);
        }

        // POST: Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Category category)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = category.Id == 0 ? "新增分類" : "編輯分類";
                return View(category);
            }

            if (category.Id == 0)
            {
                // 新增時設定 SortOrder
                if (category.SortOrder == 0)
                {
                    var maxSortOrder = await _context.Categories
                        .MaxAsync(c => (int?)c.SortOrder) ?? 0;
                    category.SortOrder = maxSortOrder + 1;
                }

                _context.Categories.Add(category);
                TempData["Toast"] = "✅ 分類新增成功！";
            }
            else
            {
                _context.Update(category);
                TempData["Toast"] = "✅ 分類更新成功！";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    TempData["Toast"] = "❌ 找不到該分類";
                    return RedirectToAction("Index");
                }

                // 先找出所有使用這個分類的商品
                var relatedMenuItems = await _context.MenuItems
                    .Where(m => m.CategoryId == id)
                    .ToListAsync();

                if (relatedMenuItems.Any())
                {
                    // 將這些商品的 CategoryId 設為 0 (沒有分類)
                    foreach (var menuItem in relatedMenuItems)
                    {
                        menuItem.CategoryId = 0;
                    }

                    _context.MenuItems.UpdateRange(relatedMenuItems);

                    TempData["Toast"] = $"🗑️ 已成功刪除分類「{category.Name}」，{relatedMenuItems.Count} 個商品已移至「未分類」";
                }
                else
                {
                    TempData["Toast"] = $"🗑️ 已成功刪除分類「{category.Name}」";
                }

                // 刪除分類
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Toast"] = $"❌ 刪除失敗：{ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}