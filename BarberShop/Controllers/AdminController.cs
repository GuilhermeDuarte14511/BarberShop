using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Dashboard Administrativo
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard Administrativo";
            return View();
        }

        // Redireciona para a tela de gerenciamento de barbeiros
        public IActionResult GerenciarBarbeiros()
        {
            return RedirectToAction("Index", "Barbeiro");
        }

        // Redireciona para a tela de gerenciamento de serviços
        public IActionResult GerenciarServicos()
        {
            return RedirectToAction("Index", "Servico");
        }

        // Exibe relatórios administrativos
        public IActionResult Relatorios()
        {
            ViewData["Title"] = "Relatórios Administrativos";
            // Aqui você pode passar dados relevantes para os relatórios, como dados combinados de barbeiros, serviços e clientes.
            return View();
        }
    }
}
