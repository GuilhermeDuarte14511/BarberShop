using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

public class AvaliacaoController : BaseController
{
    private readonly IAvaliacaoService _avaliacaoService;
    private readonly IBarbeiroService _barbeiroService;

    public AvaliacaoController(IAvaliacaoService avaliacaoService, IBarbeiroService barbeiroService, ILogService logService)
        : base(logService) // Passa o logService para a BaseController
    {
        _avaliacaoService = avaliacaoService;
        _barbeiroService = barbeiroService;
    }

    /// <summary>
    /// Exibe a página de avaliação para um agendamento específico.
    /// </summary>
    /// <param name="agendamentoId">ID do agendamento</param>
    /// <returns>View com detalhes do agendamento ou avaliação existente</returns>
    public async Task<IActionResult> Index(int agendamentoId)
    {
        try
        {
            // Verifica se já existe uma avaliação para o agendamento
            var avaliacaoExistente = await _avaliacaoService.ObterAvaliacaoPorAgendamentoIdAsync(agendamentoId);

            if (avaliacaoExistente != null)
            {
                // Loga a existência da avaliação
                await LogAsync("INFO", nameof(Index), "Avaliação já existente", $"AgendamentoId: {agendamentoId}");
                // Retorna uma view que exibe a avaliação existente
                return View("AvaliacaoExistente", avaliacaoExistente);
            }

            // Caso não exista avaliação, busca os dados do agendamento
            var agendamento = await _avaliacaoService.ObterAgendamentoPorIdAsync(agendamentoId);

            if (agendamento == null)
            {
                await LogAsync("WARN", nameof(Index), "Agendamento não encontrado", $"ID: {agendamentoId}");
                return NotFound(new { message = "Agendamento não encontrado." });
            }

            return View(agendamento); // Passa o modelo do agendamento para a view de criação
        }
        catch (Exception ex)
        {
            // Log da exceção
            await LogAsync("ERROR", nameof(Index), ex.Message, ex.StackTrace, agendamentoId.ToString());
            return StatusCode(500, "Erro interno do servidor.");
        }
    }

    /// <summary>
    /// Salva uma nova avaliação.
    /// </summary>
    /// <param name="avaliacao">Dados da avaliação</param>
    /// <returns>Resultado JSON com status da operação</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Avaliacao avaliacao)
    {
        try
        {

            var novaAvaliacao = await _avaliacaoService.AdicionarAvaliacaoAsync(avaliacao);

            await LogAsync("INFO", nameof(Create), "Avaliação salva com sucesso", $"AvaliacaoId: {novaAvaliacao.AvaliacaoId}");

            return Json(new { success = true, message = "Avaliação salva com sucesso.", data = novaAvaliacao });
        }
        catch (Exception ex)
        {
            // Log da exceção
            await LogAsync("ERROR", nameof(Create), ex.Message, ex.StackTrace);
            return Json(new { success = false, message = "Erro ao salvar a avaliação." });
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
                return RedirectToAction("Login", "Login");
            }

            // Busca as avaliações com os filtros aplicados
            var avaliacoes = await _avaliacaoService.ObterAvaliacoesFiltradasAsync(
                barbeariaId: barbeariaId.Value,
                barbeiroId: barbeiroId,
                dataInicio: dataInicio,
                dataFim: dataFim,
                notaServico: notaServico,
                notaBarbeiro: notaBarbeiro,
                observacao: observacao);

            // Total de registros para paginação
            var totalCount = avaliacoes.Count();

            // Paginação
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
                    BarbeiroNome = a.Agendamento.Barbeiro?.Nome // Ajuste para exibir o nome do barbeiro
                })
                .ToList();

            // Recupera os barbeiros para o dropdown de filtros
            var barbeiros = await _barbeiroService.ObterBarbeirosPorBarbeariaIdAsync(barbeariaId.Value);
            ViewBag.Barbeiros = barbeiros;

            // Dados para paginação
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

            return View("AvaliacoesBarbearia", pagedAvaliacoes);
        }
        catch (Exception ex)
        {
            await LogAsync("Error", nameof(AvaliacoesBarbearia), ex.Message, ex.ToString());
            return StatusCode(500, "Erro ao carregar as avaliações.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> FiltrarAvaliacoesBarbearia(int page = 1,int pageSize = 10,int? barbeiroId = null,string? dataInicio = null,string? dataFim = null,int? notaServico = null,int? notaBarbeiro = null,string? observacao = null)
    {
        try
        {
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

            if (!barbeariaId.HasValue)
            {
                return Unauthorized("Barbearia não identificada.");
            }

            // Busca as avaliações filtradas
            var avaliacoes = await _avaliacaoService.ObterAvaliacoesFiltradasAsync(
                barbeariaId: barbeariaId.Value,
                barbeiroId: barbeiroId,
                dataInicio: dataInicio,
                dataFim: dataFim,
                notaServico: notaServico,
                notaBarbeiro: notaBarbeiro,
                observacao: observacao);

            var totalCount = avaliacoes.Count();

            // Paginação
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
                    BarbeiroNome = a.Agendamento.Barbeiro?.Nome
                })
                .ToList();

            return Json(new
            {
                avaliacoes = pagedAvaliacoes,
                totalCount
            });
        }
        catch (Exception ex)
        {
            await LogAsync("Error", nameof(FiltrarAvaliacoesBarbearia), ex.Message, ex.ToString());
            return StatusCode(500, "Erro ao filtrar as avaliações.");
        }
    }


}
