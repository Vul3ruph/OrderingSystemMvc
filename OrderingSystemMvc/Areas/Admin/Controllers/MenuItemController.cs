using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;
using System.Diagnostics;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuItemController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

       
        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems
                .Include(m => m.Category) // 這樣 CategoryId = 0 的項目 Category 會是 null
                .OrderBy(m => m.CategoryId == 0 ? "未分類" : m.Category.Name)
                .ThenBy(m => m.SortOrder)
                .ToListAsync();

            return View(menuItems);
        }

        // GET: Upsert
        public async Task<IActionResult> Upsert(int id = 0)
        {
            var isCreate = id == 0;
            ViewData["Title"] = isCreate ? "新增餐點" : "編輯餐點";

            // 設定分類下拉選單
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            if (isCreate)
            {
                // 新增模式：建立空的 MenuItem
                var newItem = new MenuItem
                {
                    IsAvailable = true, // 預設為可用
                    SortOrder = await GetNextSortOrder() // 自動設定排序
                };
                return View(newItem);
            }
            else
            {
                // 編輯模式：載入現有資料
                var item = await _context.MenuItems.FindAsync(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的餐點";
                    return RedirectToAction(nameof(Index));
                }
                return View(item);
            }
        }

        // 更新 Upsert 方法以處理選項關聯
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(MenuItem item, IFormFile? formFile, List<int>? SelectedOptionIds)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var isCreate = item.Id == 0;

                    // 處理圖片上傳（你現有的代碼）
                    if (formFile != null && formFile.Length > 0)
                    {
                        // 你的圖片上傳邏輯
                         var imageUrl = await UploadImage(formFile);
                         item.ImageUrl = imageUrl;
                    }

                    // 餐點基本資訊處理
                    if (isCreate)
                    { 
                        if (item.SortOrder == 0)
                        {
                            var maxSortOrder = await _context.MenuItems
                                .Where(m => m.CategoryId == item.CategoryId)
                                .MaxAsync(m => (int?)m.SortOrder) ?? 0;
                            item.SortOrder = maxSortOrder + 1;
                        }

                    _context.MenuItems.Add(item);
                    await _context.SaveChangesAsync(); // 先保存以獲取 ID
                }
                     else
                {
                    var existingItem = await _context.MenuItems.FindAsync(item.Id);
                    if (existingItem == null)
                    {
                        TempData["ToastrType"] = "error";
                        TempData["ToastrMessage"] = "餐點不存在";
                        return RedirectToAction(nameof(Index));
                    }

                    // 更新餐點資訊
                    existingItem.Name = item.Name;
                    existingItem.Description = item.Description;
                    existingItem.Price = item.Price;
                    existingItem.CategoryId = item.CategoryId;
                    existingItem.SortOrder = item.SortOrder;
                    existingItem.IsAvailable = item.IsAvailable;

                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        existingItem.ImageUrl = item.ImageUrl;
                    }

                    _context.MenuItems.Update(existingItem);
                }

                // 處理選項關聯
                await UpdateMenuItemOptions(item.Id, SelectedOptionIds ?? new List<int>());

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ToastrType"] = "success";
                TempData["ToastrMessage"] = isCreate ? "餐點新增成功！" : "餐點更新成功！";

                return RedirectToAction(nameof(Index));
            }
        catch (Exception ex)
        {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Upsert Error: {ex.Message}");

                TempData["ToastrType"] = "error";
                TempData["ToastrMessage"] = $"操作失敗: {ex.Message}";
            }
        }

        // 重新載入頁面資料
        await LoadUpsertPageData(item);
    return View(item);
}


        // 輔助方法：檢查 MenuItem 是否存在
        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }

        // 在你的 MenuItemController 中，修改 Delete 方法
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/MenuItem/Delete")] // 明確指定路由
        public async Task<IActionResult> Delete(int id)
        {
            Debug.WriteLine($"刪除請求 ID: {id}");
            try
            {
                var item = await _context.MenuItems.FindAsync(id);
                if (item == null)
                    return Json(new { success = false, message = "找不到餐點" });

                // 判斷是否與訂單關聯
                var hasOrder = await _context.OrderItems.AnyAsync(oi => oi.MenuItemId == id);
                if (hasOrder)
                {
                    return Json(new
                    {
                        success = false,
                        message = "此餐點已有訂單記錄，無法刪除。建議改為停用。"
                    });
                }

                // 刪除圖片（如果有）
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    try { DeleteImage(item.ImageUrl); } catch { }
                }

                _context.MenuItems.Remove(item);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "餐點刪除成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("刪除失敗：" + ex.Message);
                return Json(new { success = false, message = "刪除失敗，請聯繫管理員" });
            }
        }

        // POST: Duplicate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int id)
        {
            try
            {
                var originalItem = await _context.MenuItems.FindAsync(id);
                if (originalItem == null)
                {
                    TempData["ErrorMessage"] = "找不到要複製的餐點";
                    return RedirectToAction(nameof(Index));
                }

                var duplicatedItem = new MenuItem
                {
                    Name = $"{originalItem.Name} (副本)",
                    Description = originalItem.Description,
                    Price = originalItem.Price,
                    CategoryId = originalItem.CategoryId,
                    IsAvailable = false, // 複製的餐點預設為停用
                    SortOrder = await GetNextSortOrder(),
                   
                    // 不複製圖片，避免版權問題
                };

                _context.Add(duplicatedItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "餐點複製成功，請編輯後啟用";
                return RedirectToAction(nameof(Upsert), new { id = duplicatedItem.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"複製失敗: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ToggleStatus (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var item = await _context.MenuItems.FindAsync(id);
                if (item == null)
                {
                    return Json(new { success = false, message = "找不到指定的餐點" });
                }

                item.IsAvailable = !item.IsAvailable;
                

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    isAvailable = item.IsAvailable,
                    message = item.IsAvailable ? "餐點已啟用" : "餐點已停用"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"狀態更新失敗: {ex.Message}" });
            }
        }

        //API
        [HttpGet]
        public async Task<IActionResult> GetAvailableOptions()
        {
            try
            {
                var options = await _context.Options
                    .Include(o => o.OptionItems)
                    .OrderBy(o => o.SortOrder)
                    .ThenBy(o => o.Name)
                    .Select(o => new
                    {
                        Id = o.Id,
                        Name = o.Name,
                        IsSingleChoice = o.IsSingleChoice,
                        SortOrder = o.SortOrder,
                        OptionItems = o.OptionItems
                            .OrderBy(oi => oi.Id) // 你的模型沒有 SortOrder，所以用 Id 排序
                            .Select(oi => new
                            {
                                Id = oi.Id,
                                Name = oi.Name,
                                Description = "", // 你的模型沒有 Description，所以給空字串
                                Price = oi.ExtraPrice // 注意：你的欄位是 ExtraPrice
                            }).ToList()
                    })
                    .ToListAsync();

                return Json(options);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                System.Diagnostics.Debug.WriteLine($"GetAvailableOptions Error: {ex.Message}");

                // 返回錯誤信息
                return Json(new { error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetSelectedOptions(int id)
        {
            try
            {
                if (id == 0)
                {
                    // 新增模式，返回空陣列
                    return Json(new List<object>());
                }

                // 查詢已選擇的選項
                var selectedOptions = await _context.MenuItemOptions
                    .Where(mio => mio.MenuItemId == id)
                    .Include(mio => mio.Option)
                    .ThenInclude(o => o.OptionItems)
                    .Select(mio => new
                    {
                        Id = mio.Option.Id,
                        Name = mio.Option.Name,
                        IsSingleChoice = mio.Option.IsSingleChoice,
                        SortOrder = mio.Option.SortOrder,
                        OptionItems = mio.Option.OptionItems
                            .OrderBy(oi => oi.Id)
                            .Select(oi => new
                            {
                                Id = oi.Id,
                                Name = oi.Name,
                                Description = "", // 你的模型沒有 Description
                                Price = oi.ExtraPrice // 注意：你的欄位是 ExtraPrice
                            }).ToList()
                    })
                    .ToListAsync();

                return Json(selectedOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSelectedOptions Error: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }


        #region 私有方法
        private async Task<int> GetNextSortOrder()
        {
            var maxSortOrder = await _context.MenuItems
                .MaxAsync(m => (int?)m.SortOrder) ?? 0;
            return maxSortOrder + 10; // 間隔 10，方便後續插入
        }

        private async Task<string?> UploadImage(IFormFile file)
        {
            try
            {
                // 檢查檔案大小 (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    throw new Exception("檔案大小不能超過 5MB");
                }

                // 檢查檔案類型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("只允許上傳 JPG、PNG、GIF 格式的圖片");
                }

                // 建立上傳目錄
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "menu-items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 產生唯一檔名
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 儲存檔案
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/images/menu-items/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                // 記錄錯誤 (這裡可以使用 ILogger)
                Console.WriteLine($"圖片上傳失敗: {ex.Message}");
                return null;
            }
        }

        private void DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "menu-items", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不拋出異常
                Console.WriteLine($"圖片刪除失敗: {ex.Message}");
            }
        }
        // 輔助方法：更新餐點選項關聯
        private async Task UpdateMenuItemOptions(int menuItemId, List<int> selectedOptionIds)
        {
            // 移除現有的關聯
            var existingOptions = await _context.MenuItemOptions
                .Where(mio => mio.MenuItemId == menuItemId)
                .ToListAsync();

            _context.MenuItemOptions.RemoveRange(existingOptions);

            // 添加新的關聯
            if (selectedOptionIds.Any())
            {
                var newMenuItemOptions = selectedOptionIds.Select(optionId => new MenuItemOption
                {
                    MenuItemId = menuItemId,
                    OptionId = optionId
                }).ToList();

                await _context.MenuItemOptions.AddRangeAsync(newMenuItemOptions);
            }
        }

        // 輔助方法：載入 Upsert 頁面所需資料
        private async Task LoadUpsertPageData(MenuItem item)
        {
            ViewData["Title"] = item.Id == 0 ? "新增餐點" : "編輯餐點";

            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        #endregion
    }
}