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

        // GET: MenuItem
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "餐點管理";

            var items = await _context.MenuItems
                .Include(m => m.Category)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return View(items);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(MenuItem item, IFormFile? formFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isCreate = item.Id == 0;

                    // 處理圖片上傳
                    if (formFile != null && formFile.Length > 0)
                    {
                        var imageUrl = await UploadImage(formFile);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // 如果是編輯且有舊圖片，先刪除舊圖片
                            if (!isCreate && !string.IsNullOrEmpty(item.ImageUrl))
                            {
                                DeleteImage(item.ImageUrl);
                            }
                            item.ImageUrl = imageUrl;
                        }
                    }

                    // 資料庫操作
                    if (isCreate)
                    {                   
                        _context.MenuItems.Add(item);
                      
                    }
                    else
                    {
                        // 編輯操作
                        var existingItem = await _context.MenuItems.FindAsync(item.Id);
                        if (existingItem == null)
                        {
                            TempData["ErrorMessage"] = "餐點不存在";
                            return RedirectToAction(nameof(Index));
                        }

                        // 更新屬性
                        existingItem.Name = item.Name;
                        existingItem.Description = item.Description;
                        existingItem.Price = item.Price;
                        existingItem.CategoryId = item.CategoryId;
                        existingItem.SortOrder = item.SortOrder;
                        existingItem.IsAvailable = item.IsAvailable;
                       

                        // 只有在有新圖片時才更新圖片URL
                        if (formFile != null && formFile.Length > 0 && !string.IsNullOrEmpty(item.ImageUrl))
                        {
                            existingItem.ImageUrl = item.ImageUrl;
                        }

                        _context.MenuItems.Update(existingItem);
                        TempData["SuccessMessage"] = "餐點更新成功";
                    }
                   
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                            
                        TempData["ErrorMessage"] = "更新衝突，請重新嘗試";
                        throw;
                    
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"操作失敗: {ex.Message}";

                    // 記錄詳細錯誤到日誌
                    // _logger.LogError(ex, "MenuItem Upsert failed for item: {ItemId}", item.Id);
                }
            }
            else
            {
                // ModelState 驗證失敗，顯示錯誤訊息
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                TempData["ErrorMessage"] = "表單驗證失敗，請檢查輸入內容";

                // 可選：將具體錯誤記錄到日誌
                // foreach (var error in errors)
                // {
                //     _logger.LogWarning("Validation error in field {Field}: {Errors}", 
                //         error.Field, string.Join(", ", error.Errors));
                // }
            }

            // 如果驗證失敗或發生錯誤，重新載入頁面
            ViewData["Title"] = item.Id == 0 ? "新增餐點" : "編輯餐點";

            // 設定麵包屑
            ViewBag.BreadcrumbItems = new List<dynamic>
    {
        new { Title = "餐點管理", Url = Url.Action("Index", "MenuItem", new { area = "Admin" }) },
        new { Title = item.Id == 0 ? "新增餐點" : "編輯餐點", Url = (string)null }
    };

            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

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

        #endregion
    }
}