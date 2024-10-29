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

        public PaymentService(IOptions<StripeSettings> stripeOptions)
        {
            _stripeSettings = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        /// <summary>
        /// Processa o pagamento com cartão de crédito, criando um Customer associado ao PaymentIntent.
        /// </summary>
        /// <param name="amount">Valor do pagamento.</param>
        /// <param name="clienteNome">Nome do cliente.</param>
        /// <param name="clienteEmail">E-mail do cliente.</param>
        /// <returns>ClientSecret do PaymentIntent para confirmação no frontend.</returns>
        public async Task<string> ProcessCreditCardPayment(decimal amount, string clienteNome, string clienteEmail)
        {
            try
            {
                // Cria um cliente no Stripe
                var customerOptions = new CustomerCreateOptions
                {
                    Name = clienteNome,
                    Email = clienteEmail,
                };
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                // Cria o PaymentIntent associado ao Customer criado
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "card" },
                    Customer = customer.Id
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                return paymentIntent.ClientSecret;
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Erro ao criar PaymentIntent: {ex.Message}");
                throw new Exception("Não foi possível processar o pagamento com cartão de crédito.");
            }
        }

        /// <summary>
        /// Processa o pagamento via PIX, criando um Customer e gerando um QR Code.
        /// </summary>
        /// <param name="amount">Valor do pagamento.</param>
        /// <param name="clienteNome">Nome do cliente.</param>
        /// <param name="clienteEmail">E-mail do cliente.</param>
        /// <returns>URL do QR Code para o pagamento via PIX.</returns>
        public async Task<string> ProcessPixPayment(decimal amount, string clienteNome, string clienteEmail)
        {
            try
            {
                // Cria um cliente no Stripe
                var customerOptions = new CustomerCreateOptions
                {
                    Name = clienteNome,
                    Email = clienteEmail,
                };
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                // Cria o PaymentIntent associado ao Customer para PIX
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "pix" },
                    Customer = customer.Id
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                // Verifica se o PaymentIntent possui a ação "pix_display_qr_code" e retorna a URL do QR Code
                if (paymentIntent.NextAction?.PixDisplayQrCode != null)
                {
                    return paymentIntent.NextAction.PixDisplayQrCode.Data;
                }
                else
                {
                    throw new Exception("Não foi possível obter o QR Code para o pagamento via PIX.");
                }
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Erro ao criar PaymentIntent para PIX: {ex.Message}");
                throw new Exception("Não foi possível processar o pagamento com PIX.");
            }
        }

        /// <summary>
        /// Simulação de pagamento via transferência bancária.
        /// </summary>
        /// <param name="amount">Valor do pagamento.</param>
        /// <returns>Mensagem simulada de pagamento por transferência.</returns>
        public Task<string> ProcessBankTransfer(decimal amount)
        {
            return Task.FromResult("Simulação de pagamento com transferência bancária.");
        }
    }
}
