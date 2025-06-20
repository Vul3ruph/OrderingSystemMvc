using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using OrderingSystemMvc.Models;
using System;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class OptionItemsController : Controller
{
    private readonly ApplicationDbContext _context;

    public OptionItemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Index
    public async Task<IActionResult> Index()
    {
        var optionItems = await _context.OptionItems.Include(x => x.Option).ToListAsync();
        return View(optionItems);
    }

    // Create
    public IActionResult Create()
    {
        ViewData["OptionId"] = new SelectList(_context.Options, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OptionItem item)
    {
        if (ModelState.IsValid)
        {
            _context.OptionItems.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["OptionId"] = new SelectList(_context.Options, "Id", "Name", item.OptionId);
        return View(item);
    }

    // Edit
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.OptionItems.FindAsync(id);
        if (item == null) return NotFound();
        ViewData["OptionId"] = new SelectList(_context.Options, "Id", "Name", item.OptionId);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OptionItem item)
    {
        if (id != item.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["OptionId"] = new SelectList(_context.Options, "Id", "Name", item.OptionId);
        return View(item);
    }

    // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.OptionItems
            .Include(x => x.Option)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.OptionItems.FindAsync(id);
        if (item != null)
        {
            _context.OptionItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
