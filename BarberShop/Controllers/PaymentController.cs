using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPlanoAssinaturaService _planoAssinaturaService;

        public PaymentController(IPaymentService paymentService, IPlanoAssinaturaService planoAssinaturaService)
        {
            _paymentService = paymentService;
            _planoAssinaturaService = planoAssinaturaService;
        }

        // Endpoint para criar um PaymentIntent
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequestDTO request)
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
        public async Task<IActionResult> ProcessCreditCardPayment([FromBody] CreditCardPaymentRequestDTO request)
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
        public async Task<IActionResult> ProcessPixPayment([FromBody] PixPaymentRequestDTO request)
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

        // Endpoint para sincronizar os planos de assinatura do Stripe com o banco de dados
        [HttpPost("sync-planos")]
        public async Task<IActionResult> SincronizarPlanos()
        {
            try
            {
                List<PlanoAssinaturaSistema> planosAtualizados = await _planoAssinaturaService.SincronizarPlanosComStripe();
                return Ok(planosAtualizados);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Endpoint para listar os planos de assinatura do sistema
        [HttpGet("planos")]
        public async Task<IActionResult> GetPlanos()
        {
            try
            {
                var planos = await _planoAssinaturaService.GetAllPlanosAsync();
                var planosDto = planos.Select(plano => new
                {
                    PlanoId = plano.PlanoId,
                    IdProdutoStripe = plano.IdProdutoStripe,
                    Nome = plano.Nome,
                    Descricao = plano.Descricao,
                    Valor = plano.Valor,
                    Periodicidade = plano.Periodicidade
                }).ToList();

                return Ok(planosDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("start-subscription")]
        public async Task<IActionResult> StartSubscription([FromBody] StartSubscriptionRequestDTO request)
        {
            try
            {
                var subscriptionId = await _paymentService.StartSubscription(request.PlanId, request.ClienteNome, request.ClienteEmail);
                return Ok(new { subscriptionId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        
    }
}
