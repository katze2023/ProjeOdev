
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SalonController : Controller
{
    private readonly ApplicationDbContext _context;

    public SalonController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var salons = await _context.Salons.ToListAsync();
        return View(salons);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Salon salon)
    {
        if (ModelState.IsValid)
        {
            _context.Add(salon);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(salon);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var salon = await _context.Salons.FindAsync(id);
        if (salon == null) return NotFound();

        return View(salon);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Salon salon)
    {
        if (id != salon.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(salon);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(salon);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var salon = await _context.Salons.FindAsync(id);
        if (salon == null) return NotFound();

        return View(salon);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirm(int id)
    {
        var salon = await _context.Salons.FindAsync(id);
        _context.Salons.Remove(salon);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}