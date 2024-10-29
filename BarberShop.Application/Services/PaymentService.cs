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

        public async Task<string> ProcessCreditCardPayment(decimal amount)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "card" },
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

        public async Task<string> ProcessPixPayment(decimal amount)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "pix" },
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


        public Task<string> ProcessBankTransfer(decimal amount)
        {
            return Task.FromResult("Simulação de pagamento com transferência bancária.");
        }
    }
}
