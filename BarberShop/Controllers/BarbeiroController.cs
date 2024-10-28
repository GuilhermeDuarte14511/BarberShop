using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class BarbeiroController : Controller
    {
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IBarbeiroService _barbeiroService;

        public BarbeiroController(IBarbeiroRepository barbeiroRepository, IBarbeiroService barbeiroService)
        {
            _barbeiroRepository = barbeiroRepository;
            _barbeiroService = barbeiroService;
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

        // Certifique-se de que o método está retornando a view corretamente
        public async Task<IActionResult> EscolherBarbeiro(int duracaoTotal, string servicoIds)
        {
            if (duracaoTotal <= 0)
            {
                return BadRequest("A duração dos serviços é inválida.");
            }

            var barbeiros = await _barbeiroService.ObterTodosBarbeirosAsync();
            ViewData["DuracaoTotal"] = duracaoTotal;
            ViewData["ServicoIds"] = servicoIds;
            return View("EscolherBarbeiro", barbeiros); // Certifique-se de que o nome da view está correto
        }


    }
}
