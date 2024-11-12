using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers
{
    public class ErroController : Controller
    {
        public IActionResult BarbeariaNaoEncontrada()
        {
            return View();
        }
    }
}
