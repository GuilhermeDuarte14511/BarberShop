using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarberShopMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Dashboard Administrativo
        public IActionResult Index()
        {
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ViewData["UserId"] = clienteId;
            ViewData["Title"] = "Dashboard Administrativo";           
            return View();
        }

        public IActionResult GerenciarBarbeiros()
        {
            return RedirectToAction("Index", "Barbeiro");
        }

        public IActionResult GerenciarServicos()
        {
            return RedirectToAction("Index", "Servico");
        }

        public IActionResult Relatorios()
        {
            ViewData["Title"] = "Relatórios Administrativos";
            return View();
        }
    }
}
