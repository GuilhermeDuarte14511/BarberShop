using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class ServicoController : Controller
    {
        private readonly IServicoRepository _servicoRepository;

        public ServicoController(IServicoRepository servicoRepository)
        {
            _servicoRepository = servicoRepository;
        }

        public async Task<IActionResult> Index()
        {
            var servicos = await _servicoRepository.GetAllAsync();
            return View(servicos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var servico = await _servicoRepository.GetByIdAsync(id);
            if (servico == null) return NotFound();
            return View(servico);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Servico servico)
        {
            if (ModelState.IsValid)
            {
                await _servicoRepository.AddAsync(servico);
                return RedirectToAction(nameof(Index));
            }
            return View(servico);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var servico = await _servicoRepository.GetByIdAsync(id);
            if (servico == null) return NotFound();
            return View(servico);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Servico servico)
        {
            if (id != servico.ServicoId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _servicoRepository.UpdateAsync(servico);
                return RedirectToAction(nameof(Index));
            }
            return View(servico);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var servico = await _servicoRepository.GetByIdAsync(id);
            if (servico == null) return NotFound();
            return View(servico);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _servicoRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
