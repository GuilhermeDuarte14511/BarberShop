using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarberShopMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Dashboard Administrativo
        public IActionResult Index(string barbeariaUrl)
        {
            // Recupera o ID do cliente a partir do token de autenticação
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ViewData["UserId"] = clienteId;

            // Recupera o ID e URL da barbearia da sessão
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

            // Atualiza o ViewData com a URL da barbearia recebida como parâmetro
            ViewData["BarbeariaId"] = barbeariaId;
            ViewData["BarbeariaUrl"] = barbeariaUrl;
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
