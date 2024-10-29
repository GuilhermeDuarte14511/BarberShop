namespace BarberShop.Application.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Cria um PaymentIntent para pagamentos com cartão de crédito.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <returns>ClientSecret do PaymentIntent para confirmação no frontend.</returns>
        Task<string> ProcessCreditCardPayment(decimal amount);

        /// <summary>
        /// Simula ou implementa a criação de um PaymentIntent para pagamentos via PIX.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <returns>ClientSecret do PaymentIntent (simulado).</returns>
        Task<string> ProcessPixPayment(decimal amount);

        /// <summary>
        /// Simula ou implementa a criação de um PaymentIntent para transferências bancárias.
        /// </summary>
        /// <param name="amount">Valor do pagamento em decimal.</param>
        /// <returns>ClientSecret do PaymentIntent (simulado).</returns>
        Task<string> ProcessBankTransfer(decimal amount);
    }
}
