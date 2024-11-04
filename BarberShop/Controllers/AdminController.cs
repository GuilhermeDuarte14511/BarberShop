using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public AdminController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        // Dashboard Administrativo
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard Administrativo";

            // Obter dados para os gráficos
            var agendamentosPorSemana = await _dashboardRepository.GetAgendamentosPorSemanaAsync();
            var servicosMaisSolicitados = await _dashboardRepository.GetServicosMaisSolicitadosAsync();
            var lucroPorBarbeiro = await _dashboardRepository.GetLucroPorBarbeiroAsync();
            var atendimentosPorBarbeiro = await _dashboardRepository.GetAtendimentosPorBarbeiroAsync();
            var lucroDaSemana = await _dashboardRepository.GetLucroDaSemanaAsync();
            var lucroDoMes = await _dashboardRepository.GetLucroDoMesAsync();

            // Passar os dados para a view usando o ViewBag ou ViewData
            ViewBag.AgendamentosPorSemana = agendamentosPorSemana;
            ViewBag.ServicosMaisSolicitados = servicosMaisSolicitados;
            ViewBag.LucroPorBarbeiro = lucroPorBarbeiro;
            ViewBag.AtendimentosPorBarbeiro = atendimentosPorBarbeiro;
            ViewBag.LucroDaSemana = lucroDaSemana;
            ViewBag.LucroDoMes = lucroDoMes;

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
            return View();
        }
    }
}
