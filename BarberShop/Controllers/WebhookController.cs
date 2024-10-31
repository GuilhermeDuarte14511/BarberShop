using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Infrastructure.Data; // Supondo que BarbeariaContext esteja nesse namespace
using System.Threading.Tasks;
using System;
using System.Text.Json;
using BarberShop.Application.Services;

namespace BarberShop.Controllers
{
    [Route("api/webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IAgendamentoService _agendamentoService;
        private readonly ILogger<WebhookController> _logger;
        private readonly BarbeariaContext _context; // Contexto do EF para salvar no banco

        public WebhookController(IAgendamentoService agendamentoService, ILogger<WebhookController> logger, BarbeariaContext context)
        {
            _agendamentoService = agendamentoService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> ReceiveNotification([FromBody] JsonElement webhookData)
        {
            _logger.LogInformation("Iniciando processamento de webhook...");

            try
            {
                _logger.LogDebug("Tentando extrair 'paymentId' e 'paymentStatus' do webhook data...");

                // Acessar os dados de forma segura
                if (!webhookData.TryGetProperty("data", out JsonElement dataElement) ||
                    !dataElement.TryGetProperty("id", out JsonElement idElement))
                {
                    _logger.LogWarning("Dados do webhook incompletos. 'data.id' ausente.");
                    await SaveLogAsync("WARNING", "WebhookController", "Dados do webhook incompletos", webhookData.ToString());
                    return BadRequest("Dados do webhook incompletos.");
                }

                string paymentId = idElement.GetString();
                string paymentStatus = webhookData.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : "unknown";

                _logger.LogInformation($"Recebido webhook para Payment ID: {paymentId}, Status: {paymentStatus}");
                await SaveLogAsync("INFO", "WebhookController", $"Recebido webhook para Payment ID: {paymentId}, Status: {paymentStatus}", webhookData.ToString());

                // Mapeia o status recebido para o enum StatusPagamento
                _logger.LogDebug("Mapeando status de pagamento para StatusPagamento enum...");
                StatusPagamento statusPagamento = paymentStatus.ToLower() switch
                {
                    "approved" => StatusPagamento.Aprovado,
                    "rejected" => StatusPagamento.Rejeitado,
                    _ => StatusPagamento.Pendente
                };

                _logger.LogDebug($"Status de pagamento mapeado para: {statusPagamento}");

                _logger.LogDebug("Atualizando status do agendamento no banco de dados...");
                bool updateSuccess = await _agendamentoService.UpdateAgendamentoStatusByPaymentIdAsync(paymentId, statusPagamento);

                if (!updateSuccess)
                {
                    _logger.LogWarning($"Agendamento não encontrado para o Payment ID: {paymentId}");
                    await SaveLogAsync("WARNING", "WebhookController", $"Agendamento não encontrado para o Payment ID: {paymentId}", null);
                    return NotFound("Agendamento não encontrado.");
                }

                _logger.LogInformation($"StatusPagamento atualizado para {statusPagamento} para Payment ID: {paymentId}");
                await SaveLogAsync("INFO", "WebhookController", $"StatusPagamento atualizado para {statusPagamento} para Payment ID: {paymentId}", null);

                return Ok("Notificação de pagamento processada com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar notificação de pagamento: {ex.Message}");
                await SaveLogAsync("ERROR", "WebhookController", $"Erro ao processar notificação de pagamento: {ex.Message}", webhookData.ToString());
                return StatusCode(500, "Erro interno ao processar notificação.");
            }
            finally
            {
                _logger.LogInformation("Processamento do webhook concluído.");
            }
        }

        // Método auxiliar para salvar logs no banco de dados
        private async Task SaveLogAsync(string logLevel, string source, string message, string data)
        {
            _logger.LogDebug("Salvando log no banco de dados...");
            var logEntry = new Log
            {
                LogLevel = logLevel,
                Source = source,
                Message = message,
                Data = data,
                LogDateTime = DateTime.UtcNow
            };

            _context.Logs.Add(logEntry);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Log salvo com sucesso.");
        }
    }
}
