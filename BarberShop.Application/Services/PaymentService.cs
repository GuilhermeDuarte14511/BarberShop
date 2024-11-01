using BarberShop.Application.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ILogService _logService;

        public PaymentService(IOptions<StripeSettings> stripeOptions, ILogService logService)
        {
            _stripeSettings = stripeOptions.Value;
            _logService = logService;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
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

        public async Task<string> CreatePaymentIntent(decimal amount, string currency = "brl")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Valor em centavos
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.ClientSecret; // Retorna o client_secret para o frontend
        }

    }
}
