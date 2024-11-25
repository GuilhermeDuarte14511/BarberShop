using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IPlanoAssinaturaService _planoAssinaturaService;
        private readonly IBarbeariaRepository _barbeariaRepository;

        public PaymentController(IPaymentService paymentService, IPlanoAssinaturaService planoAssinaturaService, ILogService logService, IBarbeariaRepository barbeariaRepository)
            : base(logService)
        {
            _paymentService = paymentService;
            _planoAssinaturaService = planoAssinaturaService;
            _barbeariaRepository = barbeariaRepository;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequestDTO request)
        {
            await LogAsync("Information", "PaymentController", "Iniciando criação de PaymentIntent", $"Valor: {request.Amount}, Métodos: {string.Join(", ", request.PaymentMethods)}");

            try
            {
                string clientSecret = await _paymentService.CreatePaymentIntent(request.Amount, request.PaymentMethods, request.Currency);
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao criar PaymentIntent", ex.Message);
                return BadRequest(new { error = "Não foi possível criar o PaymentIntent." });
            }
        }

        [HttpPost("create-payment-intent-barbearia")]
        public async Task<IActionResult> CreatePaymentIntentBarbearia([FromBody] PaymentIntentBarbeariaRequestDTO request)
        {
            await LogAsync("Information", "PaymentController", "Iniciando criação de PaymentIntent para barbearia",
                           $"Valor: {request.Amount}, Métodos: {string.Join(", ", request.PaymentMethods)}, Barbearia ID: {request.BarbeariaId}, Comissão: {request.CommissionPercentage * 100}%");

            try
            {
                // Buscar o accountId associado ao barbeariaId
                var accountIdStripe = await _barbeariaRepository.GetAccountIdStripeByIdAsync(int.Parse(request.BarbeariaId));

                if (string.IsNullOrEmpty(accountIdStripe))
                {
                    return BadRequest(new { error = "Conta da Stripe não encontrada para a barbearia especificada." });
                }

                // Usar o accountId obtido da barbearia para o PaymentIntent
                string clientSecret = await _paymentService.CreatePaymentIntentBarbearia(
                    request.Amount,
                    request.PaymentMethods,
                    request.Currency,
                    accountIdStripe,
                    request.CommissionPercentage
                );

                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao criar PaymentIntent para barbearia", ex.Message);
                return BadRequest(new { error = "Não foi possível criar o PaymentIntent para a barbearia." });
            }
        }


        [HttpPost("process-credit-card")]
        public async Task<IActionResult> ProcessCreditCardPayment([FromBody] CreditCardPaymentRequestDTO request)
        {
            await LogAsync("Information", "PaymentController", "Processando pagamento com cartão de crédito", $"Cliente: {request.ClienteNome}, Email: {request.ClienteEmail}, Valor: {request.Amount}");

            try
            {
                string clientSecret = await _paymentService.ProcessCreditCardPayment(request.Amount, request.ClienteNome, request.ClienteEmail);
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao processar pagamento com cartão de crédito", ex.Message);
                return BadRequest(new { error = "Não foi possível processar o pagamento com cartão de crédito." });
            }
        }

        [HttpPost("process-pix")]
        public async Task<IActionResult> ProcessPixPayment([FromBody] PixPaymentRequestDTO request)
        {
            await LogAsync("Information", "PaymentController", "Processando pagamento via PIX", $"Cliente: {request.ClienteNome}, Email: {request.ClienteEmail}, Valor: {request.Amount}");

            try
            {
                string qrCodeData = await _paymentService.ProcessPixPayment(request.Amount, request.ClienteNome, request.ClienteEmail);
                return Ok(new { qrCodeData });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao processar pagamento via PIX", ex.Message);
                return BadRequest(new { error = "Não foi possível processar o pagamento via PIX." });
            }
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequest request)
        {
            await LogAsync("Information", "PaymentController", "Iniciando reembolso", $"ID do pagamento: {request.PaymentId}, Valor: {request.Amount}");

            try
            {
                string refundStatus = await _paymentService.RefundPaymentAsync(request.PaymentId, request.Amount);
                return Ok(new { refundStatus });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao processar reembolso", ex.Message);
                return BadRequest(new { error = "Não foi possível processar o reembolso." });
            }
        }

        [HttpPost("sync-planos")]
        public async Task<IActionResult> SincronizarPlanos()
        {
            await LogAsync("Information", "PaymentController", "Sincronizando planos de assinatura com Stripe", null);

            try
            {
                List<PlanoAssinaturaSistema> planosAtualizados = await _planoAssinaturaService.SincronizarPlanosComStripe();
                return Ok(planosAtualizados);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao sincronizar planos de assinatura", ex.Message);
                return BadRequest(new { error = "Não foi possível sincronizar os planos de assinatura." });
            }
        }

        [HttpGet("planos")]
        public async Task<IActionResult> GetPlanos()
        {
            await LogAsync("Information", "PaymentController", "Obtendo planos de assinatura", null);

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
                    Periodicidade = plano.Periodicidade,
                    PriceId = plano.PriceId
                }).ToList();

                return Ok(planosDto);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao obter planos de assinatura", ex.Message);
                return BadRequest(new { error = "Não foi possível obter os planos de assinatura." });
            }
        }

        [HttpPost("start-subscription")]
        public async Task<IActionResult> StartSubscription([FromBody] StartSubscriptionRequestDTO request)
        {
            if (request == null)
                return BadRequest(new { error = "O corpo da requisição não pode estar vazio." });

            await LogAsync("Information", "PaymentController", "Iniciando assinatura", $"ID do Plano: {request.PlanId}, Cliente: {request.ClienteNome}");

            try
            {
                var subscriptionId = await _paymentService.StartSubscription(request.PlanId, request.PriceId, request.ClienteNome, request.ClienteEmail);
                return Ok(new { subscriptionId });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao iniciar assinatura", ex.Message);
                return BadRequest(new { error = "Não foi possível iniciar a assinatura." });
            }
        }

        [HttpPost("save-payment")]
        public async Task<IActionResult> SavePayment([FromBody] SavePaymentRequestDTO request)
        {
            await LogAsync("Information", "PaymentController", "Salvando detalhes do pagamento", $"Cliente ID: {request.ClienteId}, Valor: {request.ValorPago}");

            try
            {
                var paymentDetails = new PaymentDetails
                {
                    ClienteId = request.ClienteId,
                    NomeCliente = request.NomeCliente,
                    EmailCliente = request.EmailCliente,
                    TelefoneCliente = request.TelefoneCliente,
                    ValorPago = request.ValorPago,
                    PaymentId = request.PaymentId,
                    StatusPagamento = request.StatusPagamento,
                    DataPagamento = DateTime.UtcNow,
                    BarbeariaId = request.BarbeariaId
                };

                await _paymentService.SavePayment(paymentDetails);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "PaymentController", "Erro ao salvar detalhes do pagamento", ex.Message);
                return BadRequest(new { error = "Não foi possível salvar os detalhes do pagamento." });
            }
        }
    }
}
