using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SalonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalonController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var salons = _context.Salons.ToList();
            return View(salons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Salon salon)
        {
            if (ModelState.IsValid)
            {
                _context.Salons.Add(salon);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(salon);
        }

        public IActionResult Edit(int id)
        {
            var salon = _context.Salons.Find(id);
            if (salon == null) return NotFound();

            return View(salon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Salon salon)
        {
            if (ModelState.IsValid)
            {
                _context.Salons.Update(salon);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(salon);
        }

        public IActionResult Delete(int id)
        {
            var salon = _context.Salons.Find(id);
            if (salon == null) return NotFound();

            return View(salon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var salon = _context.Salons.Find(id);
            if (salon == null) return NotFound();

            _context.Salons.Remove(salon);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
