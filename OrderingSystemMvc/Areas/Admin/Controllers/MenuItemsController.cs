using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Models;
using System;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuItemsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var menuItems = _context.MenuItems.Include(m => m.Category).ToList();
            return View(menuItems);
        }

        public IActionResult Upsert(int? id)
        {
            var categories = _context.Categories.OrderBy(c => c.SortOrder).ToList();
            ViewBag.Categories = categories;

            if (id == null || id == 0)
            {
                return View(new MenuItem());
            }

            var item = _context.MenuItems.Find(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(MenuItem item, IFormFile? formFile)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            // 檔案上傳處理
            if (formFile != null && formFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/menu");
                Directory.CreateDirectory(uploadsFolder); // 若資料夾不存在就建立

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(fileStream);
                }

                // 儲存圖片路徑（相對路徑）
                item.ImageUrl = "/images/menu/" + uniqueFileName;
            }

            if (item.Id == 0)
            {
                _context.MenuItems.Add(item);
            }
            else
            {
                _context.MenuItems.Update(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null) return NotFound();

            _context.MenuItems.Remove(item);
            _context.SaveChanges();
            TempData["Toast"] = "🗑️ 餐點已刪除";
            return RedirectToAction("Index");
        }
    }
}
