using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BarberShop.Application.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Domain.Entities;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using BarberShop.Application.Services;

namespace BarberShop.Controllers
{
    [Route("api/webhook")]
    public class WebhookController : BaseController
    {
        private readonly IAgendamentoService _agendamentoService;

        public WebhookController(IAgendamentoService agendamentoService, ILogger<WebhookController> logger, BarbeariaContext context)
            : base(logger, context)
        {
            _agendamentoService = agendamentoService;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> ReceiveNotification([FromBody] JsonElement webhookData)
        {
            Logger.LogInformation("Iniciando processamento de webhook...");

            try
            {
                Logger.LogDebug("Tentando extrair dados do webhook...");

                // Extraindo os campos principais
                string action = webhookData.TryGetProperty("action", out JsonElement actionElement) ? actionElement.GetString() : null;
                string type = webhookData.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : null;

                // Verificar se o campo "data.id" existe
                if (!webhookData.TryGetProperty("data", out JsonElement dataElement) ||
                    !dataElement.TryGetProperty("id", out JsonElement idElement))
                {
                    Logger.LogWarning("Dados do webhook incompletos. 'data.id' ausente.");
                    await SaveLogAsync("WARNING", "WebhookController", "Dados do webhook incompletos", webhookData.ToString(), null);
                    return BadRequest("Dados do webhook incompletos.");
                }

                string resourceId = idElement.GetString();
                Logger.LogInformation($"Recebido webhook - Tipo: {type}, Ação: {action}, Resource ID: {resourceId}");
                await SaveLogAsync("INFO", "WebhookController", $"Recebido webhook - Tipo: {type}, Ação: {action}, Resource ID: {resourceId}", webhookData.ToString(), resourceId);

                // Processamento de acordo com o tipo de notificação
                StatusPagamento statusPagamento = StatusPagamento.Pendente; // Valor padrão

                if (type == "payment")
                {
                    statusPagamento = action switch
                    {
                        "payment.created" => StatusPagamento.Pendente,
                        "payment.updated" => StatusPagamento.Pendente,
                        "payment.approved" => StatusPagamento.Aprovado,
                        "payment.rejected" => StatusPagamento.Rejeitado,
                        _ => StatusPagamento.Pendente
                    };

                    bool updateSuccess = await _agendamentoService.UpdateAgendamentoStatusByPaymentIdAsync(resourceId, statusPagamento);

                    if (!updateSuccess)
                    {
                        Logger.LogWarning($"Agendamento não encontrado para o Payment ID: {resourceId}");
                        await SaveLogAsync("WARNING", "WebhookController", $"Agendamento não encontrado para o Payment ID: {resourceId}", null, resourceId);
                        return NotFound("Agendamento não encontrado.");
                    }
                }
                else if (type == "automatic-payments" && action == "card.updated")
                {
                    string newCardId = dataElement.TryGetProperty("new_card_id", out JsonElement newCardIdElement) ? newCardIdElement.ToString() : null;
                    string oldCardId = dataElement.TryGetProperty("old_card_id", out JsonElement oldCardIdElement) ? oldCardIdElement.ToString() : null;

                    Logger.LogInformation($"Cartão atualizado - Novo Card ID: {newCardId}, Antigo Card ID: {oldCardId}");
                    await SaveLogAsync("INFO", "WebhookController", $"Cartão atualizado - Novo Card ID: {newCardId}, Antigo Card ID: {oldCardId}", webhookData.ToString(), resourceId);
                }
                else if (type == "stop_delivery_op_wh" && action == "Created")
                {
                    string paymentId = dataElement.TryGetProperty("payment_id", out JsonElement paymentIdElement) ? paymentIdElement.ToString() : null;
                    string merchantOrder = dataElement.TryGetProperty("merchant_order", out JsonElement merchantOrderElement) ? merchantOrderElement.ToString() : null;

                    Logger.LogWarning($"Alerta de fraude - Merchant Order: {merchantOrder}, Payment ID: {paymentId}");
                    await SaveLogAsync("WARNING", "WebhookController", $"Alerta de fraude - Merchant Order: {merchantOrder}, Payment ID: {paymentId}", webhookData.ToString(), resourceId);
                }
                else
                {
                    Logger.LogWarning($"Tipo de notificação não suportado - Tipo: {type}, Ação: {action}");
                    await SaveLogAsync("WARNING", "WebhookController", $"Tipo de notificação não suportado - Tipo: {type}, Ação: {action}", webhookData.ToString(), resourceId);
                    return BadRequest("Tipo de notificação não suportado.");
                }

                Logger.LogInformation($"Processamento do webhook concluído para Resource ID: {resourceId}");
                return Ok("Notificação processada com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro ao processar notificação de pagamento: {ex.Message}");
                await SaveLogAsync("ERROR", "WebhookController", $"Erro ao processar notificação: {ex.Message}", webhookData.ToString(), null);
                return StatusCode(500, "Erro interno ao processar notificação.");
            }
        }
    }
}
