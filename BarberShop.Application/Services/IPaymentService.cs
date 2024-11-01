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
        /// <returns>ClientSecret do PaymentIntent para confirmação no frontend.</returns>
        Task<string> CreatePaymentIntent(decimal amount, string currency = "brl");
    }
}
