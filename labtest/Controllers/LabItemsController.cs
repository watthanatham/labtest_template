using labtest.Data;
using labtest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace labtest.Controllers;

public class LabItemsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.LabItems.AsNoTracking().OrderByDescending(item => item.Id).ToListAsync());

    public IActionResult Create() => View(new LabItem());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] LabItem item)
    {
        if (!ModelState.IsValid)
            return View(item);

        db.LabItems.Add(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.LabItems.FindAsync(id);
        if (item is null)
            return NotFound();

        db.LabItems.Remove(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
