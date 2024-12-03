using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShopMVC.Controllers
{
    [Authorize(Roles = "Admin,Barbeiro")]
    public class AdminController : BaseController
    {
        private readonly IAvaliacaoService _avaliacaoService;
        private readonly IBarbeiroService _barbeiroService;

        public AdminController(ILogService logService, IAvaliacaoService avaliacaoService, IBarbeiroService barbeiroService)
            : base(logService) // Passa o logService para a BaseController
        {
            _avaliacaoService = avaliacaoService;
            _barbeiroService = barbeiroService;
        }

        // Dashboard Administrativo
        public async Task<IActionResult> IndexAsync(string barbeariaUrl)
        {
            try
            {
                var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                ViewData["UserId"] = clienteId;

                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                ViewData["BarbeariaId"] = barbeariaId;
                ViewData["BarbeariaUrl"] = barbeariaUrl;
                ViewData["Title"] = "Dashboard Administrativo";

                await LogAsync("INFO", nameof(IndexAsync), "Dashboard acessado com sucesso", $"ClienteId: {clienteId}, BarbeariaId: {barbeariaId}");
                return View();
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(IndexAsync), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar o dashboard.");
            }
        }

        public async Task<IActionResult> GerenciarBarbeirosAsync()
        {
            try
            {
                await LogAsync("INFO", nameof(GerenciarBarbeirosAsync), "Redirecionando para gestão de barbeiros", null);
                return RedirectToAction("Index", "Barbeiro");
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(GerenciarBarbeirosAsync), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao acessar a gestão de barbeiros.");
            }
        }

        public async Task<IActionResult> GerenciarServicosAsync()
        {
            try
            {
                await LogAsync("INFO", nameof(GerenciarServicosAsync), "Redirecionando para gestão de serviços", null);
                return RedirectToAction("Index", "Servico");
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(GerenciarServicosAsync), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao acessar a gestão de serviços.");
            }
        }

        public async Task<IActionResult> RelatoriosAsync()
        {
            try
            {
                ViewData["Title"] = "Relatórios Administrativos";
                await LogAsync("INFO", nameof(RelatoriosAsync), "Relatórios acessados com sucesso", null);
                return View();
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(RelatoriosAsync), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao acessar relatórios.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AvaliacoesBarbearia(int page = 1, int pageSize = 10, int? barbeiroId = null, string? dataInicio = null, string? dataFim = null, int? notaServico = null, int? notaBarbeiro = null, string? observacao = null)
        {
            try
            {
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    await LogAsync("WARN", nameof(AvaliacoesBarbearia), "Barbearia não identificada na sessão", null);
                    return RedirectToAction("Login", "Login");
                }

                var avaliacoes = await _avaliacaoService.ObterAvaliacoesFiltradasAsync(
                    barbeariaId: barbeariaId.Value,
                    barbeiroId: barbeiroId,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    notaServico: notaServico,
                    notaBarbeiro: notaBarbeiro,
                    observacao: observacao);

                var totalCount = avaliacoes.Count();

                var pagedAvaliacoes = avaliacoes
                .OrderByDescending(a => a.DataAvaliado)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.AvaliacaoId,
                    AgendamentoId = a.AgendamentoId,
                    NotaServico = a.NotaServico,
                    NotaBarbeiro = a.NotaBarbeiro,
                    Observacao = a.Observacao,
                    DataAvaliado = a.DataAvaliado,
                    BarbeiroNome = a.Agendamento.Barbeiro?.Nome,
                    ClienteNome = a.Agendamento.Cliente.Nome,
                    ClienteEmail = a.Agendamento.Cliente.Email
                })
                .ToList();

                var barbeiros = await _barbeiroService.ObterBarbeirosPorBarbeariaIdAsync(barbeariaId.Value);
                var barbeirosDto = barbeiros.Select(b => new BarbeiroDto
                {
                    BarbeiroId = b.BarbeiroId,
                    Nome = b.Nome
                }).ToList();

                ViewBag.Barbeiros = barbeirosDto;

                ViewData["CurrentPage"] = page;
                ViewData["PageSize"] = pageSize;
                ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

                await LogAsync("INFO", nameof(AvaliacoesBarbearia), "Avaliações carregadas com sucesso", $"BarbeariaId: {barbeariaId}, TotalCount: {totalCount}");
                return View("AvaliacoesBarbearia", pagedAvaliacoes);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(AvaliacoesBarbearia), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar as avaliações.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarAvaliacoesBarbearia(int page = 1, int pageSize = 10, int? barbeiroId = null, string? dataInicio = null, string? dataFim = null, int? notaServico = null, int? notaBarbeiro = null, string? observacao = null)
        {
            try
            {
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    await LogAsync("WARN", nameof(FiltrarAvaliacoesBarbearia), "Barbearia não identificada na sessão", null);
                    return Unauthorized("Barbearia não identificada.");
                }

                var avaliacoes = await _avaliacaoService.ObterAvaliacoesFiltradasAsync(
                    barbeariaId: barbeariaId.Value,
                    barbeiroId: barbeiroId,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    notaServico: notaServico,
                    notaBarbeiro: notaBarbeiro,
                    observacao: observacao);

                var totalCount = avaliacoes.Count();

                var pagedAvaliacoes = avaliacoes
                .OrderByDescending(a => a.DataAvaliado)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.AvaliacaoId,
                    AgendamentoId = a.AgendamentoId,
                    NotaServico = a.NotaServico,
                    NotaBarbeiro = a.NotaBarbeiro,
                    Observacao = a.Observacao,
                    DataAvaliado = a.DataAvaliado,
                    BarbeiroNome = a.Agendamento.Barbeiro?.Nome,
                    ClienteNome = a.Agendamento.Cliente.Nome,
                    ClienteEmail = a.Agendamento.Cliente.Email
                })
                .ToList();

                await LogAsync("INFO", nameof(FiltrarAvaliacoesBarbearia), "Filtro de avaliações aplicado com sucesso", $"BarbeariaId: {barbeariaId}, TotalCount: {totalCount}");
                return Json(new
                {
                    avaliacoes = pagedAvaliacoes,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(FiltrarAvaliacoesBarbearia), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao filtrar as avaliações.");
            }
        }
    }
}
