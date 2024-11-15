using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Cria um PaymentIntent para pagamentos com cartão de crédito, associado a um cliente.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <param name="clienteNome">Nome do cliente.</param>
        /// <param name="clienteEmail">E-mail do cliente.</param>
        /// <returns>ClientSecret do PaymentIntent para confirmação no frontend.</returns>
        Task<string> ProcessCreditCardPayment(decimal amount, string clienteNome, string clienteEmail);

        /// <summary>
        /// Cria um PaymentIntent para pagamentos via PIX, associado a um cliente.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <param name="clienteNome">Nome do cliente.</param>
        /// <param name="clienteEmail">E-mail do cliente.</param>
        /// <returns>URL do QR Code para o pagamento via PIX.</returns>
        Task<string> ProcessPixPayment(decimal amount, string clienteNome, string clienteEmail);

        /// <summary>
        /// Simula um pagamento via transferência bancária.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <returns>Mensagem simulada de confirmação de transferência bancária.</returns>
        Task<string> ProcessBankTransfer(decimal amount);

        /// <summary>
        /// Cria um PaymentIntent para um valor específico, sem associar a um cliente.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <param name="paymentMethods">Lista de métodos de pagamento aceitos.</param>
        /// <param name="currency">Moeda do pagamento (padrão: "brl").</param>
        /// <returns>ClientSecret do PaymentIntent para confirmação no frontend.</returns>
        Task<string> CreatePaymentIntent(decimal amount, List<string> paymentMethods, string currency = "brl");

        /// <summary>
        /// Cria um PaymentIntent para um pagamento direcionado à conta de uma barbearia, com uma comissão para a conta principal.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <param name="paymentMethods">Lista de métodos de pagamento aceitos.</param>
        /// <param name="currency">Moeda do pagamento.</param>
        /// <param name="barbeariaAccountId">ID da conta Stripe conectada da barbearia.</param>
        /// <param name="commissionPercentage">Percentual de comissão a ser retido pela conta principal.</param>
        /// <returns>ClientSecret do PaymentIntent para confirmação no frontend.</returns>
        Task<string> CreatePaymentIntentBarbearia(decimal amount, List<string> paymentMethods, string currency, string barbeariaAccountId, decimal? commissionPercentage);

        /// <summary>
        /// Processa o reembolso de um pagamento específico.
        /// </summary>
        /// <param name="paymentId">ID do pagamento a ser reembolsado.</param>
        /// <param name="amount">Valor do reembolso em centavos (opcional). Se null, o valor total será reembolsado.</param>
        /// <returns>Status do reembolso.</returns>
        Task<string> RefundPaymentAsync(string paymentId, long? amount = null);

        Task<List<PlanoAssinaturaSistema>> SincronizarPlanosComStripe();

        Task<string> StartSubscription(string planId, string priceId, string clienteNome, string clienteEmail);

        Task SavePayment(PaymentDetails paymentDetails);
    }
}
