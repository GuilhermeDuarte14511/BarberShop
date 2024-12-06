using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class PagamentoController : BaseController
    {
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly ILogService _logService;
        private readonly IOnboardingService _onboardingService;

        public PagamentoController(IPagamentoRepository pagamentoRepository, ILogService logService, IOnboardingService onboardingService)
            : base(logService)
        {
            _pagamentoRepository = pagamentoRepository;
            _logService = logService;
            _onboardingService = onboardingService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Barbearia não encontrada na sessão." });
                }

                int usuarioId = ObterUsuarioIdLogado();
                bool isOnboardingComplete = await _onboardingService.VerificarProgressoAsync(usuarioId, "Pagamentos");

                ViewData["ShowOnboarding"] = !isOnboardingComplete ? "True" : "False";

                var pagamentos = await _pagamentoRepository.GetAllPagamentosByBarbeariaIdAsync(barbeariaId.Value);
                return View(pagamentos);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Index), "Erro ao carregar a lista de pagamentos", ex.Message);
                return View("Error", "Ocorreu um erro ao carregar os pagamentos.");
            }
        }


        public async Task<IActionResult> Detalhes(int id)
        {
            try
            {
                var pagamento = await _pagamentoRepository.GetByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound(new { success = false, message = "Pagamento não encontrado." });
                }

                var detalhes = new
                {
                    pagamento.PagamentoId,
                    NomeCliente = pagamento.Agendamento?.Cliente?.Nome ?? "N/A",
                    pagamento.ValorPago,
                    pagamento.StatusPagamento,
                    DataPagamento = pagamento.DataPagamento.HasValue ? pagamento.DataPagamento.Value.ToString("dd/MM/yyyy") : "N/A"
                };

                return Json(detalhes);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Detalhes), "Erro ao carregar detalhes do pagamento", ex.Message, id.ToString());
                return Json(new { success = false, message = "Erro ao carregar detalhes.", error = ex.Message });
            }
        }

        public async Task<IActionResult> SolicitarReembolso(int id)
        {
            try
            {
                var pagamento = await _pagamentoRepository.GetByIdAsync(id);
                if (pagamento == null)
                {
                    await LogAsync("INFO", nameof(SolicitarReembolso), "Pagamento não encontrado para reembolso", $"Pagamento ID: {id}");
                    return NotFound(new { success = false, message = "Pagamento não encontrado." });
                }

                if (pagamento.StatusPagamento != StatusPagamento.Aprovado)
                {
                    await LogAsync("INFO", nameof(SolicitarReembolso), "Tentativa de reembolso em pagamento não aprovado", $"Pagamento ID: {id}");
                    return BadRequest(new { success = false, message = "O pagamento não está elegível para reembolso." });
                }

                pagamento.StatusPagamento = StatusPagamento.Reembolsado;
                await _pagamentoRepository.UpdateAsync(pagamento);

                await LogAsync("INFO", nameof(SolicitarReembolso), "Reembolso realizado com sucesso", $"Pagamento ID: {id}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(SolicitarReembolso), "Erro ao solicitar reembolso", ex.Message, id.ToString());
                return View("Error", "Ocorreu um erro ao solicitar o reembolso.");
            }
        }
    }
}
