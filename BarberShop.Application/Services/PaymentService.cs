using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ILogService _logService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly BarbeariaContext _context; // Injete o contexto do banco de dados

        public PaymentService(IOptions<StripeSettings> stripeOptions, BarbeariaContext context, IPaymentRepository paymentRepository, ILogService logService)
        {
            _stripeSettings = stripeOptions.Value;
            _context = context;
            _paymentRepository = paymentRepository;
            _logService = logService;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<string> CreatePaymentIntent(decimal amount, List<string> paymentMethods, string currency = "brl")
        {
            await _logService.SaveLogAsync("Information", "PaymentService", "Iniciando criação de PaymentIntent.", $"Valor: {amount}, Métodos de pagamento: {string.Join(", ", paymentMethods)}");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Valor em centavos
                Currency = currency,
                PaymentMethodTypes = paymentMethods.Count > 0 ? paymentMethods : null
            };

            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                await _logService.SaveLogAsync("Information", "PaymentService", "PaymentIntent criado com sucesso.", $"ID do PaymentIntent: {paymentIntent.Id}");

                return paymentIntent.ClientSecret; // Retorna o client_secret para o frontend
            }
            catch (StripeException ex)
            {
                await _logService.SaveLogAsync("Error", "PaymentService", "Erro ao criar PaymentIntent.", ex.Message);
                throw new Exception("Não foi possível criar o PaymentIntent.");
            }
        }

        public async Task<string> ProcessCreditCardPayment(decimal amount, string clienteNome, string clienteEmail)
        {
            await _logService.SaveLogAsync("Information", "PaymentService", "Iniciando processamento de pagamento com cartão de crédito.", $"Cliente: {clienteNome}, Email: {clienteEmail}");

            try
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Name = clienteNome,
                    Email = clienteEmail,
                };
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                await _logService.SaveLogAsync("Information", "PaymentService", "Cliente criado com sucesso no Stripe.", $"ID do cliente: {customer.Id}");

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "card" },
                    Customer = customer.Id
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                await _logService.SaveLogAsync("Information", "PaymentService", "PaymentIntent criado com sucesso.", $"ID do PaymentIntent: {paymentIntent.Id}");

                return paymentIntent.ClientSecret;
            }
            catch (StripeException ex)
            {
                await _logService.SaveLogAsync("Error", "PaymentService", "Erro ao criar PaymentIntent para cartão de crédito.", ex.Message);
                throw new Exception("Não foi possível processar o pagamento com cartão de crédito.");
            }
        }

        public async Task<string> ProcessPixPayment(decimal amount, string clienteNome, string clienteEmail)
        {
            await _logService.SaveLogAsync("Information", "PaymentService", "Iniciando processamento de pagamento via PIX.", $"Cliente: {clienteNome}, Email: {clienteEmail}");

            try
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Name = clienteNome,
                    Email = clienteEmail,
                };
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                await _logService.SaveLogAsync("Information", "PaymentService", "Cliente criado com sucesso no Stripe para pagamento PIX.", $"ID do cliente: {customer.Id}");

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "pix" },
                    Customer = customer.Id
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                await _logService.SaveLogAsync("Information", "PaymentService", "PaymentIntent criado com sucesso para PIX.", $"ID do PaymentIntent: {paymentIntent.Id}");

                if (paymentIntent.NextAction?.PixDisplayQrCode != null)
                {
                    await _logService.SaveLogAsync("Information", "PaymentService", "QR Code gerado com sucesso para pagamento PIX.", null);
                    return paymentIntent.NextAction.PixDisplayQrCode.Data;
                }
                else
                {
                    await _logService.SaveLogAsync("Warning", "PaymentService", "Não foi possível obter o QR Code para pagamento PIX.", null);
                    throw new Exception("Não foi possível obter o QR Code para o pagamento via PIX.");
                }
            }
            catch (StripeException ex)
            {
                await _logService.SaveLogAsync("Error", "PaymentService", "Erro ao criar PaymentIntent para PIX.", ex.Message);
                throw new Exception("Não foi possível processar o pagamento com PIX.");
            }
        }

        public async Task<string> ProcessBankTransfer(decimal amount)
        {
            await _logService.SaveLogAsync("Information", "PaymentService", "Simulando pagamento via transferência bancária.", $"Valor: {amount}");
            return "Simulação de pagamento com transferência bancária.";
        }

        /// <summary>
        /// Processa o reembolso de um pagamento específico.
        /// </summary>
        /// <param name="paymentId">ID do pagamento a ser reembolsado.</param>
        /// <param name="amount">Valor do reembolso em centavos (opcional). Se null, o valor total será reembolsado.</param>
        /// <returns>Status do reembolso.</returns>
        public async Task<string> RefundPaymentAsync(string paymentId, long? amount = null)
        {
            await _logService.SaveLogAsync("Information", "PaymentService", "Iniciando processo de reembolso.", $"ID do pagamento: {paymentId}, Valor: {amount ?? 0}");

            try
            {
                var refundService = new RefundService();
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentId,
                    Amount = amount // Se null, reembolsa o valor total; caso contrário, define o valor parcial em centavos
                };

                var refund = await refundService.CreateAsync(options);

                await _logService.SaveLogAsync("Information", "PaymentService", "Reembolso processado com sucesso.", $"ID do Reembolso: {refund.Id}, Status: {refund.Status}");

                return refund.Status;
            }
            catch (StripeException ex)
            {
                await _logService.SaveLogAsync("Error", "PaymentService", "Erro ao processar o reembolso.", ex.Message);
                throw new Exception("Não foi possível processar o reembolso.");
            }
        }

        public async Task<List<PlanoAssinaturaSistema>> SincronizarPlanosComStripe()
        {
            var service = new ProductService();
            var products = await service.ListAsync(new ProductListOptions
            {
                Active = true,
                Limit = 100
            });

            var planosAtualizados = new List<PlanoAssinaturaSistema>();

            foreach (var product in products)
            {
                var priceService = new PriceService();
                var prices = await priceService.ListAsync(new PriceListOptions
                {
                    Product = product.Id,
                    Limit = 1
                });

                if (prices.Data.Count > 0)
                {
                    var price = prices.Data[0];

                    // Encontre o plano com base no `IdProdutoStripe`, pois `PlanoId` é gerado pelo banco.
                    var planoExistente = await _context.PlanoAssinaturaSistema
                        .FirstOrDefaultAsync(plano => plano.IdProdutoStripe == product.Id);

                    if (planoExistente == null)
                    {
                        var novoPlano = new PlanoAssinaturaSistema
                        {
                            Nome = product.Name,
                            Descricao = product.Description,
                            IdProdutoStripe = product.Id, // Armazena o ID do Stripe
                            Valor = (decimal)(price.UnitAmount / 100.0), // Converte de centavos para unidade monetária
                            Periodicidade = price.Recurring.Interval
                        };

                        _context.PlanoAssinaturaSistema.Add(novoPlano);
                        planosAtualizados.Add(novoPlano);
                    }
                    else
                    {
                        // Atualiza o plano existente
                        planoExistente.Nome = product.Name;
                        planoExistente.Descricao = product.Description;
                        planoExistente.IdProdutoStripe = product.Id;
                        planoExistente.Valor = (decimal)(price.UnitAmount / 100.0);
                        planoExistente.Periodicidade = price.Recurring.Interval;

                        planosAtualizados.Add(planoExistente);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Remove planos que não estão mais no Stripe
            var idsPlanosAtuais = new HashSet<string>(products.Select(p => p.Id));
            var planosParaRemover = _context.PlanoAssinaturaSistema
                .Where(plano => !idsPlanosAtuais.Contains(plano.IdProdutoStripe));

            _context.PlanoAssinaturaSistema.RemoveRange(planosParaRemover);
            await _context.SaveChangesAsync();

            return planosAtualizados;
        }

        public async Task<string> StartSubscription(string planId, string priceId, string clienteNome, string clienteEmail)
        {
            // Criação do cliente no Stripe
            var customerOptions = new CustomerCreateOptions
            {
                Name = clienteNome,
                Email = clienteEmail,
            };
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(customerOptions);

            // Criação da assinatura no Stripe com `PriceId`
            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId // Usando o PriceId aqui em vez do PlanId
                    }
                },
                PaymentBehavior = "default_incomplete", // Gera client_secret se precisar confirmação
                Expand = new List<string> { "latest_invoice.payment_intent" } // Expande para obter o client_secret
            };

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

            // Pega o client_secret para confirmação de pagamento inicial, se necessário
            var clientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret;

            // Retorna o ID da assinatura e o client_secret se disponível
            return clientSecret ?? subscription.Id;
        }

        public async Task SavePayment(PaymentDetails paymentDetails)
        {
            var pagamento = new PagamentoAssinatura
            {
                ClienteId = paymentDetails.ClienteId,
                NomeCliente = paymentDetails.NomeCliente,
                EmailCliente = paymentDetails.EmailCliente,
                TelefoneCliente = paymentDetails.TelefoneCliente,
                ValorPago = paymentDetails.ValorPago,
                PaymentId = paymentDetails.PaymentId,
                StatusPagamento = paymentDetails.StatusPagamento,
                DataPagamento = paymentDetails.DataPagamento,
                BarbeariaId = paymentDetails.BarbeariaId
            };

            await _paymentRepository.AddAsync(pagamento);
            await _paymentRepository.SaveChangesAsync();
        }

    }
}
