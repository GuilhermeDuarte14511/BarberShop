using BarberShop.Application.Services;
using BarberShop.Application.DTOs;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MercadoPago.Resource.Payment;
using System.Text.Json;
using MercadoPago.Client;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
        // Pegue o token do Mercado Pago de uma variável de ambiente
        MercadoPagoConfig.AccessToken = Environment.GetEnvironmentVariable("MERCADO_PAGO_ACCESS_TOKEN")
                                         ?? "TEST-236275902252089-103018-3e74429f349b5f03d14f49344da52ec9-430792758"; // Remova esta linha em produção
    }

    [HttpPost("create-preference")]
    public async Task<IActionResult> CreatePreference([FromBody] JsonElement data)
    {
        try
        {
            decimal amount = data.GetProperty("amount").GetDecimal();
            string description = data.GetProperty("description").GetString();

            var preferenceId = await _paymentService.CreateMercadoPagoPreference(amount, description);
            return Ok(new { preferenceId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar preferência: {ex.Message}");
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
            var paymentClient = new PaymentClient();
            Payment payment = await paymentClient.CreateAsync(paymentCreateRequest, requestOptions);

            if (payment.Status == "approved")
            {
                return Ok(new { status = "approved", paymentId = payment.Id });
            }
            else
            {
                // Retorna um motivo detalhado de rejeição para exibir ao usuário
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
            Console.WriteLine($"Erro ao processar pagamento: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }


    // Método auxiliar para mapear status_detail para mensagens personalizadas
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

// DTO para Requisição de Pagamento
public class PaymentRequestDTO
{
    public decimal TransactionAmount { get; set; }
    public string Token { get; set; }
    public string Description { get; set; }
    public int Installments { get; set; }
    public string PaymentMethodId { get; set; }
    public PaymentPayer Payer { get; set; }
}

// DTO para Payer
public class PaymentPayer
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public Identification Identification { get; set; }
}

// DTO para Identificação
public class Identification
{
    public string Type { get; set; }
    public string Number { get; set; }
}
