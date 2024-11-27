using BarberShop.Application.Services;
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
    [Authorize(Roles = "Admin,Barbeiro")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IRelatorioPersonalizadoRepository _relatorioPersonalizadoRepository;

        public DashboardController(
            IDashboardRepository dashboardRepository,
            IRelatorioPersonalizadoRepository relatorioPersonalizadoRepository,
            ILogService logService) : base(logService)
        {
            _dashboardRepository = dashboardRepository;
            _relatorioPersonalizadoRepository = relatorioPersonalizadoRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

                if (barbeariaId == null)
                {
                    await LogAsync("Warning", "Dashboard", "BarbeariaId não encontrado na sessão.", DateTime.UtcNow.ToString("o"));
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                ViewData["UserId"] = usuarioId;
                ViewData["BarbeariaId"] = barbeariaId;
                ViewData["BarbeariaUrl"] = barbeariaUrl;
                ViewData["Title"] = "Dashboard Administrativo";

                await LogAsync("Info", "Dashboard", "Acesso ao painel administrativo", DateTime.UtcNow.ToString("o"), $"UsuarioId: {usuarioId}");
                return View();
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "Dashboard", $"Erro ao carregar o dashboard: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return RedirectToAction("ErroInterno", "Erro");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var barbeariaIdClaim = User.FindFirst("BarbeariaId")?.Value;

                if (string.IsNullOrEmpty(barbeariaIdClaim))
                {
                    await LogAsync("Warning", "GetDashboardData", "BarbeariaId não encontrado nos claims.", DateTime.UtcNow.ToString("o"));
                    return BadRequest(new { message = "ID da barbearia não encontrado nos claims." });
                }

                int barbeariaId = int.Parse(barbeariaIdClaim);
                var barbeiroIdClaim = User.FindFirst("BarbeiroId")?.Value;
                int? barbeiroId = string.IsNullOrEmpty(barbeiroIdClaim) ? (int?)null : int.Parse(barbeiroIdClaim);

                var agendamentosPorSemana = await _dashboardRepository.GetAgendamentosPorSemanaAsync(barbeariaId, barbeiroId);
                var servicosMaisSolicitados = await _dashboardRepository.GetServicosMaisSolicitadosAsync(barbeariaId, barbeiroId);
                var lucroPorBarbeiro = await _dashboardRepository.GetLucroPorBarbeiroAsync(barbeariaId, barbeiroId);
                var atendimentosPorBarbeiro = await _dashboardRepository.GetAtendimentosPorBarbeiroAsync(barbeariaId, barbeiroId);
                var lucroDaSemana = await _dashboardRepository.GetLucroDaSemanaAsync(barbeariaId, barbeiroId);
                var lucroDoMes = await _dashboardRepository.GetLucroDoMesAsync(barbeariaId, barbeiroId);

                await LogAsync("Info", "GetDashboardData", "Dados do dashboard carregados", DateTime.UtcNow.ToString("o"), $"BarbeariaId: {barbeariaId}");

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
            catch (Exception ex)
            {
                await LogAsync("Error", "GetDashboardData", $"Erro ao carregar dados do dashboard: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return StatusCode(500, new { message = "Erro interno ao processar os dados do dashboard." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetCustomReportData(string reportType, int periodDays)
        {
            try
            {
                var barbeariaIdClaim = User.FindFirst("BarbeariaId")?.Value;

                if (string.IsNullOrEmpty(barbeariaIdClaim) || !int.TryParse(barbeariaIdClaim, out var barbeariaId))
                {
                    return BadRequest(new { message = "ID da barbearia não encontrado nos claims." });
                }

                var barbeiroIdClaim = User.FindFirst("BarbeiroId")?.Value;
                int? barbeiroId = string.IsNullOrEmpty(barbeiroIdClaim) ? (int?)null : int.Parse(barbeiroIdClaim);

                var data = await _dashboardRepository.GetCustomReportDataAsync(barbeariaId, reportType, periodDays, barbeiroId);
                await LogAsync("Info", "GetCustomReportData", "Relatório personalizado gerado", DateTime.UtcNow.ToString("o"), $"BarbeariaId: {barbeariaId}");
                return Json(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "GetCustomReportData", $"Erro ao obter relatório personalizado: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return StatusCode(500, new { message = "Erro ao processar o relatório." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomReport([FromBody] RelatorioPersonalizado relatorio)
        {
            try
            {
                await _relatorioPersonalizadoRepository.SalvarRelatorioPersonalizadoAsync(relatorio);
                await LogAsync("Info", "SaveCustomReport", "Relatório salvo com sucesso.", DateTime.UtcNow.ToString("o"), $"UsuarioId: {relatorio.UsuarioId}");
                return Ok(new { message = "Relatório salvo com sucesso" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "SaveCustomReport", $"Erro ao salvar relatório: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return StatusCode(500, new { message = "Erro ao salvar o relatório." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadUserReports(int usuarioId)
        {
            try
            {
                var reports = await _relatorioPersonalizadoRepository.ObterRelatoriosPorUsuarioAsync(usuarioId);
                await LogAsync("Info", "LoadUserReports", "Relatórios carregados com sucesso.", DateTime.UtcNow.ToString("o"), $"UsuarioId: {usuarioId}");
                return Json(reports);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "LoadUserReports", $"Erro ao carregar relatórios: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return StatusCode(500, new { message = "Erro ao carregar relatórios." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveChartPositions([FromBody] List<GraficoPosicao> posicoes)
        {
            try
            {
                await _dashboardRepository.SaveChartPositionsAsync(posicoes);
                await LogAsync("Info", "SaveChartPositions", "Posições dos gráficos salvas com sucesso.", DateTime.UtcNow.ToString("o"));
                return Ok(new { message = "Posições dos gráficos salvas com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "SaveChartPositions", $"Erro ao salvar posições dos gráficos: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return StatusCode(500, new { message = "Erro ao salvar posições dos gráficos." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartPositions()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var posicoes = await _dashboardRepository.GetChartPositionsAsync(usuarioId);
                await LogAsync("Info", "GetChartPositions", "Posições dos gráficos carregadas com sucesso.", DateTime.UtcNow.ToString("o"), $"UsuarioId: {usuarioId}");
                return Json(posicoes.Select(p => p.GraficoId));
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "GetChartPositions", $"Erro ao carregar posições dos gráficos: {ex.Message}", DateTime.UtcNow.ToString("o"));
                return StatusCode(500, new { message = "Erro ao carregar posições dos gráficos." });
            }
        }
    }
}
