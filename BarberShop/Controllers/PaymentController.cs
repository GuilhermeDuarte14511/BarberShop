using BarberShop.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Endpoint para criar um PaymentIntent
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequest request)
        {
            try
            {
                string clientSecret = await _paymentService.CreatePaymentIntent(request.Amount, request.PaymentMethods, request.Currency);
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Endpoint para processar pagamento com cartão de crédito
        [HttpPost("process-credit-card")]
        public async Task<IActionResult> ProcessCreditCardPayment([FromBody] CreditCardPaymentRequest request)
        {
            try
            {
                string clientSecret = await _paymentService.ProcessCreditCardPayment(request.Amount, request.ClienteNome, request.ClienteEmail);
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Endpoint para processar pagamento via PIX
        [HttpPost("process-pix")]
        public async Task<IActionResult> ProcessPixPayment([FromBody] PixPaymentRequest request)
        {
            try
            {
                string qrCodeData = await _paymentService.ProcessPixPayment(request.Amount, request.ClienteNome, request.ClienteEmail);
                return Ok(new { qrCodeData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Endpoint para reembolso
        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequest request)
        {
            try
            {
                string refundStatus = await _paymentService.RefundPaymentAsync(request.PaymentId, request.Amount);
                return Ok(new { refundStatus });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // Modelo de requisição para criar um PaymentIntent
    public class PaymentIntentRequest
    {
        public decimal Amount { get; set; }
        public List<string> PaymentMethods { get; set; } = new List<string> { "card" }; // Default to "card"
        public string Currency { get; set; } = "brl";
    }

    // Modelo de requisição para pagamento com cartão de crédito
    public class CreditCardPaymentRequest
    {
        public decimal Amount { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteEmail { get; set; }
    }

    // Modelo de requisição para pagamento via PIX
    public class PixPaymentRequest
    {
        public decimal Amount { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteEmail { get; set; }
    }

    // Modelo de requisição para reembolso
    public class RefundRequest
    {
        public string PaymentId { get; set; }
        public long? Amount { get; set; } // Valor opcional para reembolso parcial em centavos
    }
}
