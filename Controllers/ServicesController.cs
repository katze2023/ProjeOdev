
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterManagement.Controllers
{
    [Authorize(Roles = "Admin")] // admin yetkisi ile yönetim; istersen kaldır veya değiştir
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Services
        [AllowAnonymous] // hizmet listesi herkese açık olabilir; isteğe göre kaldır
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                                         .Include(s => s.Salon) // salon bilgisini göstermek istersen
                                         .ToListAsync();
            return View(services);
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                                        .Include(s => s.Salon)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // GET: Services/Create
        public IActionResult Create()
        {
            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Salons, "Id", "Ad");
            return View();
        }

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,DurationMinutes,Price,SalonId")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Salons, "Id", "Ad", service.SalonId);
            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Salons, "Id", "Ad", service.SalonId);
            return View(service);
        }

        // POST: Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DurationMinutes,Price,SalonId")] Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Salons, "Id", "Ad", service.SalonId);
            return View(service);
        }

        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                                        .Include(s => s.Salon)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
