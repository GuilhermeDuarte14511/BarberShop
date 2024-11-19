using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

public class AvaliacaoController : BaseController
{
    private readonly IAvaliacaoService _avaliacaoService;

    public AvaliacaoController(IAvaliacaoService avaliacaoService, ILogService logService)
        : base(logService) // Passa o logService para a BaseController
    {
        _avaliacaoService = avaliacaoService;
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
}
