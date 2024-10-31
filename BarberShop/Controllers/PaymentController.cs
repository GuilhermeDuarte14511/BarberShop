using BarberShop.Application.Services;
using BarberShop.Application.DTOs;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Adicione essa dependência
using System;
using System.Threading.Tasks;
using MercadoPago.Resource.Payment;
using System.Text.Json;
using MercadoPago.Client;
using BarberShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger; // Adicione o ILogger

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("create-preference")]
    public async Task<IActionResult> CreatePreference([FromBody] JsonElement data)
    {
        try
        {
            decimal amount = data.GetProperty("amount").GetDecimal();
            string description = data.GetProperty("description").GetString();

            _logger.LogInformation("Valor recebido no endpoint create-preference: Amount={Amount}", amount);

            var preferenceId = await _paymentService.CreateMercadoPagoPreference(amount, description);
            return Ok(new { preferenceId });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao criar preferência: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("process_payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDTO paymentRequest)
    {
        var requestOptions = new RequestOptions();
        requestOptions.CustomHeaders.Add("x-idempotency-key", Guid.NewGuid().ToString());

        var paymentCreateRequest = new PaymentCreateRequest
        {
            TransactionAmount = paymentRequest.TransactionAmount,
            Token = paymentRequest.Token,
            Description = paymentRequest.Description,
            StatementDescriptor = "BarberShop",
            NotificationUrl = "https://barbeariashopsandbox2.azurewebsites.net/api/webhook/payment",
            ExternalReference = Guid.NewGuid().ToString(),
            Installments = paymentRequest.Installments,
            PaymentMethodId = paymentRequest.PaymentMethodId,
            Payer = new PaymentPayerRequest
            {
                Email = paymentRequest.Payer.Email,
                Identification = new IdentificationRequest
                {
                    Type = paymentRequest.Payer.Identification.Type,
                    Number = paymentRequest.Payer.Identification.Number,
                },
                FirstName = paymentRequest.Payer.FirstName
            }
        };

        try
        {
            _logger.LogInformation("Iniciando pagamento para o valor de {TransactionAmount}", paymentRequest.TransactionAmount);

            var paymentClient = new PaymentClient();
            Payment payment = await paymentClient.CreateAsync(paymentCreateRequest, requestOptions);

            _logger.LogInformation("Status do pagamento: {Status}, PaymentId: {PaymentId}", payment.Status, payment.Id);

            if (payment.Status == "approved")
            {
                return Ok(new { status = "approved", paymentId = payment.Id });
            }
            else
            {
                _logger.LogWarning("Pagamento rejeitado: {StatusDetail}", payment.StatusDetail);

                return BadRequest(new
                {
                    status = payment.Status,
                    details = payment.StatusDetail,
                    message = GetRejectionMessage(payment.StatusDetail)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao processar pagamento: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    private string GetRejectionMessage(string statusDetail)
    {
        return statusDetail switch
        {
            "cc_rejected_insufficient_amount" => "Saldo insuficiente no cartão.",
            "cc_rejected_max_attempts" => "Limite de tentativas de pagamento excedido.",
            "cc_rejected_call_for_authorize" => "Entre em contato com o emissor do cartão para autorizar o pagamento.",
            "cc_rejected_card_disabled" => "Cartão desativado. Entre em contato com o emissor.",
            "cc_rejected_blacklist" => "Cartão bloqueado. Entre em contato com o emissor.",
            "cc_rejected_high_risk" => "Pagamento recusado por alto risco. Tente outro método.",
            "cc_rejected_other_reason" => "O cartão não pôde processar o pagamento. Tente outro método.",
            _ => "O pagamento foi recusado. Verifique os dados e tente novamente."
        };
    }
}
