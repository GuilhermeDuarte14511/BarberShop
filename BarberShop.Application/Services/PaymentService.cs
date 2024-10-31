using BarberShop.Application.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using MercadoPago.Config;
using MercadoPago.Client.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MercadoPago.Client.Preference;

namespace BarberShop.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly string _mercadoPagoAccessToken;

        public PaymentService(IOptions<StripeSettings> stripeOptions)
        {
            _stripeSettings = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            // Configura o token de acesso do Mercado Pago
            _mercadoPagoAccessToken = "TEST-236275902252089-103018-3e74429f349b5f03d14f49344da52ec9-430792758"; // Substitua pelo seu Access Token real
            MercadoPagoConfig.AccessToken = _mercadoPagoAccessToken;
        }

        #region Métodos do Stripe

        public async Task<string> ProcessCreditCardPayment(decimal amount, string clienteNome, string clienteEmail)
        {
            try
            {
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Name = clienteNome,
                    Email = clienteEmail,
                });

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "card" },
                    Customer = customer.Id
                });

                return paymentIntent.ClientSecret;
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Erro ao criar PaymentIntent: {ex.Message}");
                throw new Exception("Não foi possível processar o pagamento com cartão de crédito.");
            }
        }

        public async Task<string> ProcessPixPayment(decimal amount, string clienteNome, string clienteEmail)
        {
            try
            {
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Name = clienteNome,
                    Email = clienteEmail,
                });

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = "brl",
                    PaymentMethodTypes = new List<string> { "pix" },
                    Customer = customer.Id
                });

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

        #endregion

        #region Métodos do Mercado Pago

        public async Task<string> ProcessMercadoPagoCreditCardPayment(
    decimal amount, string clienteEmail, string paymentMethodId, string cardToken, int installments = 1, string issuer = null,
    string MPHiddenInputToken = null, string MPHiddenInputPaymentMethod = null, string MPHiddenInputProcessingMode = null, string MPHiddenInputMerchantAccountId = null)
        {
            try
            {
                var paymentRequest = new PaymentCreateRequest
                {
                    TransactionAmount = amount, // Certifique-se que isso é um decimal
                    Description = "Pagamento com Cartão de Crédito - BarberShop",
                    PaymentMethodId = paymentMethodId,
                    Token = cardToken,
                    Payer = new PaymentPayerRequest
                    {
                        Email = clienteEmail
                    },
                    Installments = installments,
                    IssuerId = issuer
                };

                // Incluindo parâmetros adicionais, caso fornecidos
                if (!string.IsNullOrEmpty(MPHiddenInputToken))
                    paymentRequest.Token = MPHiddenInputToken;
                if (!string.IsNullOrEmpty(MPHiddenInputPaymentMethod))
                    paymentRequest.PaymentMethodId = MPHiddenInputPaymentMethod;
                if (!string.IsNullOrEmpty(MPHiddenInputProcessingMode))
                    paymentRequest.ProcessingMode = MPHiddenInputProcessingMode;
                if (!string.IsNullOrEmpty(MPHiddenInputMerchantAccountId))
                    paymentRequest.MerchantAccountId = MPHiddenInputMerchantAccountId;

                var client = new PaymentClient();
                var payment = await client.CreateAsync(paymentRequest);

                return payment.Status == "approved" ? "Pagamento aprovado" : "Pagamento pendente";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar pagamento com Mercado Pago: {ex.Message}");
                throw new Exception("Não foi possível processar o pagamento com cartão de crédito.");
            }
        }

        public async Task<string> ProcessMercadoPagoPixPayment(
            decimal amount, string clienteNome, string clienteEmail,
            string MPHiddenInputToken = null, string MPHiddenInputPaymentMethod = null, string MPHiddenInputProcessingMode = null, string MPHiddenInputMerchantAccountId = null)
        {
            try
            {
                var paymentRequest = new PaymentCreateRequest
                {
                    TransactionAmount = amount,
                    Description = "Pagamento com PIX - BarberShop",
                    PaymentMethodId = "pix",
                    Payer = new PaymentPayerRequest
                    {
                        Email = clienteEmail
                    }
                };

                // Incluindo parâmetros adicionais, caso fornecidos
                if (!string.IsNullOrEmpty(MPHiddenInputToken))
                    paymentRequest.Token = MPHiddenInputToken;
                if (!string.IsNullOrEmpty(MPHiddenInputPaymentMethod))
                    paymentRequest.PaymentMethodId = MPHiddenInputPaymentMethod;
                if (!string.IsNullOrEmpty(MPHiddenInputProcessingMode))
                    paymentRequest.ProcessingMode = MPHiddenInputProcessingMode;
                if (!string.IsNullOrEmpty(MPHiddenInputMerchantAccountId))
                    paymentRequest.MerchantAccountId = MPHiddenInputMerchantAccountId;

                var client = new PaymentClient();
                var payment = await client.CreateAsync(paymentRequest);

                if (payment.Status == "pending" && payment.PointOfInteraction != null && payment.PointOfInteraction.TransactionData.QrCodeBase64 != null)
                {
                    return payment.PointOfInteraction.TransactionData.QrCodeBase64;
                }

                throw new Exception("Não foi possível gerar o QR Code para pagamento via PIX.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar pagamento via PIX com Mercado Pago: {ex.Message}");
                throw new Exception("Não foi possível processar o pagamento via PIX.");
            }
        }



        public async Task<string> CreateMercadoPagoPreference(decimal amount, string description)
        {
            try
            {
                // Log dos parâmetros recebidos
                Console.WriteLine($"Criando preferência do Mercado Pago com: Amount={amount}, Description={description}");

                var preferenceRequest = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = description,
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = amount // Certifique-se que amount é decimal
                    }
                },
                    BackUrls = new PreferenceBackUrlsRequest
                    {
                        Success = "https://www.seusite.com/sucesso",
                        Failure = "https://www.seusite.com/erro",
                        Pending = "https://www.seusite.com/pendente"
                    },
                    AutoReturn = "approved",
                    // Defina NotificationUrl se desejar receber notificações
                    NotificationUrl = "https://www.seusite.com/notifications"
                };

                // Log do objeto `preferenceRequest` que será enviado
                Console.WriteLine($"PreferenceRequest: {Newtonsoft.Json.JsonConvert.SerializeObject(preferenceRequest)}");

                var preferenceClient = new PreferenceClient();
                var preference = await preferenceClient.CreateAsync(preferenceRequest);

                // Log do resultado recebido
                Console.WriteLine($"Preferência criada com sucesso. Preference ID: {preference.Id}");

                return preference.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar preferência do Mercado Pago: {ex.Message}");
                throw new Exception("Não foi possível criar a preferência de pagamento.");
            }
        }



        #endregion
    }
}
