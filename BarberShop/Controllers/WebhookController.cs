using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using System.Threading.Tasks;
using System;
using BarberShop.Application.Services;

namespace BarberShop.Controllers
{
    [Route("api/webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IAgendamentoService _agendamentoService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IAgendamentoService agendamentoService, ILogger<WebhookController> logger)
        {
            _agendamentoService = agendamentoService;
            _logger = logger;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> ReceiveNotification([FromBody] dynamic webhookData)
        {
            try
            {
                // Extrai o paymentId e o status do pagamento do webhook
                string paymentId = webhookData?.data?.id;
                string paymentStatus = webhookData?.data?.status;

                if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(paymentStatus))
                {
                    _logger.LogWarning("Dados do webhook incompletos. paymentId ou paymentStatus ausentes.");
                    return BadRequest("Dados do webhook incompletos.");
                }

                _logger.LogInformation($"Recebido webhook para Payment ID: {paymentId}, Status: {paymentStatus}");

                // Mapeia o status recebido para o enum StatusPagamento
                StatusPagamento statusPagamento = paymentStatus.ToLower() switch
                {
                    "approved" => StatusPagamento.Aprovado,
                    "rejected" => StatusPagamento.Rejeitado,
                    _ => StatusPagamento.Pendente
                };

                bool updateSuccess = await _agendamentoService.UpdateAgendamentoStatusByPaymentIdAsync(paymentId, statusPagamento);

                if (!updateSuccess)
                {
                    _logger.LogWarning($"Agendamento não encontrado para o Payment ID: {paymentId}");
                    return NotFound("Agendamento não encontrado.");
                }

                _logger.LogInformation($"StatusPagamento atualizado para {statusPagamento} para Payment ID: {paymentId}");
                return Ok("Notificação de pagamento processada com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar notificação de pagamento: {ex.Message}");
                return StatusCode(500, "Erro interno ao processar notificação.");
            }
        }
    }
}
