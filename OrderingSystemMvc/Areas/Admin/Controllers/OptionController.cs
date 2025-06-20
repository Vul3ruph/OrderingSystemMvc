using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;
using System;

namespace OrderingSystemMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Option/Index
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "選項大類管理";
            return View(await _context.Options.OrderBy(o => o.Name).ToListAsync());
        }

        // GET: Option/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "新增選項大類";
            return View();
        }

        // POST: Option/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Option option)
        {
            if (ModelState.IsValid)
            {
                _context.Options.Add(option);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "新增選項大類";
            return View(option);
        }

        // GET: Option/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var option = await _context.Options.FindAsync(id);
            if (option == null) return NotFound();

            ViewData["Title"] = "編輯選項大類";
            return View(option);
        }

        // POST: Option/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Option option)
        {
            if (id != option.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(option);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OptionExists(option.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Title"] = "編輯選項大類";
            return View(option);
        }

        // GET: Option/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var option = await _context.Options.FirstOrDefaultAsync(o => o.Id == id);
            if (option == null) return NotFound();

            ViewData["Title"] = "刪除選項大類";
            return View(option);
        }

        // POST: Option/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option != null)
            {
                _context.Options.Remove(option);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OptionExists(int id)
        {
            return _context.Options.Any(e => e.Id == id);
        }
    }
}
