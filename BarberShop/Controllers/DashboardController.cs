using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IRelatorioPersonalizadoRepository _relatorioPersonalizadoRepository;

        public DashboardController(IDashboardRepository dashboardRepository, IRelatorioPersonalizadoRepository relatorioPersonalizadoRepository)
        {
            _dashboardRepository = dashboardRepository;
            _relatorioPersonalizadoRepository = relatorioPersonalizadoRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Recupera o ID do cliente a partir do token de autenticação
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ViewData["UserId"] = clienteId;

            // Recupera o ID e URL da barbearia da sessão
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
            var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

            if (barbeariaId == null)
            {
                return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
            }

            // Adiciona ao ViewData para uso na view
            ViewData["BarbeariaId"] = barbeariaId;
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            ViewData["Title"] = "Dashboard Administrativo";

            return View();
        }

        // Método para retornar dados de gráficos
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            // Recupera o ID e URL da barbearia da sessão
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

            if (barbeariaId == null)
            {
                return BadRequest(new { message = "ID da barbearia não encontrado na sessão." });
            }

            var agendamentosPorSemana = await _dashboardRepository.GetAgendamentosPorSemanaAsync(barbeariaId.Value);
            var servicosMaisSolicitados = await _dashboardRepository.GetServicosMaisSolicitadosAsync(barbeariaId.Value);
            var lucroPorBarbeiro = await _dashboardRepository.GetLucroPorBarbeiroAsync(barbeariaId.Value);
            var atendimentosPorBarbeiro = await _dashboardRepository.GetAtendimentosPorBarbeiroAsync(barbeariaId.Value);
            var lucroDaSemana = await _dashboardRepository.GetLucroDaSemanaAsync(barbeariaId.Value);
            var lucroDoMes = await _dashboardRepository.GetLucroDoMesAsync(barbeariaId.Value);

            return Json(new
            {
                AgendamentosPorSemana = agendamentosPorSemana,
                ServicosMaisSolicitados = servicosMaisSolicitados,
                LucroPorBarbeiro = lucroPorBarbeiro,
                AtendimentosPorBarbeiro = atendimentosPorBarbeiro,
                LucroDaSemana = lucroDaSemana,
                LucroDoMes = lucroDoMes
            });
        }

        // Método para retornar dados de relatórios personalizados
        [HttpGet]
        public async Task<IActionResult> GetCustomReportData(string reportType, int periodDays)
        {
            // Recupera o ID da barbearia da sessão
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

            if (barbeariaId == null)
            {
                return BadRequest(new { message = "ID da barbearia não encontrado na sessão." });
            }

            try
            {
                var data = await _dashboardRepository.GetCustomReportDataAsync(barbeariaId.Value, reportType, periodDays);
                return Json(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter dados de relatório personalizado: {ex.Message}");
                return StatusCode(500, new { message = "Ocorreu um erro ao processar o relatório. Por favor, tente novamente." });
            }
        }

        // Método para salvar o relatório personalizado
        [HttpPost]
        public async Task<IActionResult> SaveCustomReport([FromBody] RelatorioPersonalizado relatorio)
        {
            try
            {
                await _relatorioPersonalizadoRepository.SalvarRelatorioPersonalizadoAsync(relatorio);
                return Ok(new { message = "Relatório salvo com sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar relatório: {ex.Message}");
                return StatusCode(500, new { message = "Ocorreu um erro ao salvar o relatório." });
            }
        }

        // Método para carregar relatórios salvos do usuário atual
        [HttpGet]
        public async Task<IActionResult> LoadUserReports(int usuarioId)
        {
            var reports = await _relatorioPersonalizadoRepository.ObterRelatoriosPorUsuarioAsync(usuarioId);
            return Json(reports);
        }

        // Método para salvar as posições dos gráficos
        [HttpPost]
        public async Task<IActionResult> SaveChartPositions([FromBody] List<GraficoPosicao> posicoes)
        {
            try
            {
                await _dashboardRepository.SaveChartPositionsAsync(posicoes);
                return Ok(new { message = "Posições dos gráficos salvas com sucesso." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar posições dos gráficos: {ex.Message}");
                return StatusCode(500, new { message = "Ocorreu um erro ao salvar as posições dos gráficos." });
            }
        }

        // Método para obter as posições dos gráficos
        [HttpGet]
        public async Task<IActionResult> GetChartPositions()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posicoes = await _dashboardRepository.GetChartPositionsAsync(usuarioId);
            return Json(posicoes.Select(p => p.GraficoId));
        }
    }
}
