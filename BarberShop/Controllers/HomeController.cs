using Microsoft.AspNetCore.Mvc;
using BarberShop.Domain.Interfaces;
using System.Threading.Tasks;

namespace BarberShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBarbeariaRepository _barbeariaRepository;

        public HomeController(IBarbeariaRepository barbeariaRepository)
        {
            _barbeariaRepository = barbeariaRepository;
        }

        // Ação para exibir a lista de barbearias
        public async Task<IActionResult> Index()
        {
            // Obtém todas as barbearias ativas
            var barbearias = await _barbeariaRepository.ObterTodasAtivasAsync();
            return View(barbearias);
        }
    }
}
