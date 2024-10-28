using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class BarbeiroController : Controller
    {
        private readonly IBarbeiroRepository _barbeiroRepository;

        public BarbeiroController(IBarbeiroRepository barbeiroRepository)
        {
            _barbeiroRepository = barbeiroRepository;
        }

        public async Task<IActionResult> Index()
        {
            var barbeiros = await _barbeiroRepository.GetAllAsync();
            return View(barbeiros);
        }

        public async Task<IActionResult> Details(int id)
        {
            var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
            if (barbeiro == null) return NotFound();
            return View(barbeiro);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Barbeiro barbeiro)
        {
            if (ModelState.IsValid)
            {
                await _barbeiroRepository.AddAsync(barbeiro);
                return RedirectToAction(nameof(Index));
            }
            return View(barbeiro);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
            if (barbeiro == null) return NotFound();
            return View(barbeiro);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Barbeiro barbeiro)
        {
            if (id != barbeiro.BarbeiroId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _barbeiroRepository.UpdateAsync(barbeiro);
                return RedirectToAction(nameof(Index));
            }
            return View(barbeiro);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
            if (barbeiro == null) return NotFound();
            return View(barbeiro);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _barbeiroRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
