using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Models;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookies")]
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
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    SortOrder = c.SortOrder,                   
                    MenuItems = c.MenuItems // 包含 MenuItems
                })
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
                    category.SortOrder = category.SortOrder == 0 ? 0 : category.SortOrder;
                   
                }
                else
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

                // 編輯時只更新需要的欄位
                var existingCategory = await _context.Categories.FindAsync(category.Id);
                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name;
                    existingCategory.SortOrder = category.SortOrder;
                    // 如果有其他欄位，也在這裡更新

                    _context.Categories.Update(existingCategory);
                    TempData["Toast"] = "✅ 分類更新成功！";
                }
                else
                {
                    TempData["Toast"] = "❌ 找不到要更新的分類！";
                    return RedirectToAction("Index");
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        

        [HttpPost]
        public async Task<IActionResult> UpdateSortOrder([FromBody] List<SortOrderUpdate> updates)
        {
            try
            {
                foreach (var update in updates)
                {
                    var category = await _context.Categories.FindAsync(update.Id);
                    if (category != null)
                    {
                        category.SortOrder = update.SortOrder;
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }



        // 簡化你的 Delete POST 方法，直接重定向而不返回 JSON

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // 防止刪除系統預設分類
            if (id == 0)
            {
                TempData["Toast"] = "❌ 無法刪除系統預設分類";
                return RedirectToAction("Index");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var category = await _context.Categories
                    .Include(c => c.MenuItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["Toast"] = "❌ 找不到該分類";
                    return RedirectToAction("Index");
                }

                // 檢查是否有相關商品
                var relatedMenuItems = category.MenuItems?.ToList() ?? new List<MenuItem>();

                if (relatedMenuItems.Any())
                {
                    // 將商品移至「未分類」(CategoryId = 0)
                    foreach (var menuItem in relatedMenuItems)
                    {
                        menuItem.CategoryId = 0;
                    }
                    _context.MenuItems.UpdateRange(relatedMenuItems);
                }

                // 刪除分類
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 顯示成功訊息
                var message = relatedMenuItems.Any()
                    ? $"✅ 已刪除分類「{category.Name}」，{relatedMenuItems.Count} 個商品已移至「未分類」"
                    : $"✅ 已刪除分類「{category.Name}」";

                TempData["Toast"] = message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Toast"] = "❌ 刪除失敗，請稍後再試";
                return RedirectToAction("Index");
            }
        }

    }
}