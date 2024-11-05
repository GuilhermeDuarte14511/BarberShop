using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShopMVC.Controllers;
using Microsoft.AspNetCore.Mvc;

public class AvaliacaoController : BaseController
{
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IEmailService _emailService;

    public AvaliacaoController(IAvaliacaoRepository avaliacaoRepository, IAgendamentoRepository agendamentoRepository, IEmailService emailService, ILogService logService)
        : base(logService) // Passando logService para a base
    {
        _avaliacaoRepository = avaliacaoRepository;
        _agendamentoRepository = agendamentoRepository;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index(int agendamentoId)
    {
        try
        {
            var agendamento = await _agendamentoRepository.GetDataAvaliacaoAsync(agendamentoId);
            if (agendamento == null)
            {
                return NotFound(); // Retorna 404 se o agendamento não for encontrado
            }

            return View(agendamento); // Passa o agendamento para a view
        }
        catch (Exception ex)
        {
            // Loga a exceção
            await LogAsync("ERROR", nameof(Index), ex.Message, ex.StackTrace);
            return StatusCode(500, "Ocorreu um erro ao processar sua solicitação."); // Retorna um erro genérico
        }
    }


    [HttpPost]
    public async Task<IActionResult> Create(Avaliacao avaliacao)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _avaliacaoRepository.AddAsync(avaliacao);
                // Enviar email solicitando avaliação, se necessário
                // await _emailService.EnviarEmailAvaliacaoAsync(avaliacao);
                return Json(new { success = true, message = "Avaliação enviada com sucesso!" });
            }
            return Json(new { success = false, message = "Erro ao enviar a avaliação." });
        }
        catch (Exception ex)
        {
            // Loga a exceção
            await LogAsync("ERROR", nameof(Create), ex.Message, ex.StackTrace);
            return Json(new { success = false, message = "Ocorreu um erro ao enviar a avaliação." });
        }
    }
}
