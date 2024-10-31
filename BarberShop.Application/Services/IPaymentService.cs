namespace BarberShop.Application.Services
{

    public interface IPaymentService
    {
        // Métodos do Stripe
        Task<string> ProcessCreditCardPayment(decimal amount, string clienteNome, string clienteEmail);
        Task<string> ProcessPixPayment(decimal amount, string clienteNome, string clienteEmail);
        Task<string> ProcessBankTransfer(decimal amount);

        // Métodos do Mercado Pago com novos parâmetros opcionais
        Task<string> ProcessMercadoPagoCreditCardPayment(
            decimal amount, string clienteEmail, string paymentMethodId, string cardToken, int installments = 1, string issuer = null,
            string MPHiddenInputToken = null, string MPHiddenInputPaymentMethod = null, string MPHiddenInputProcessingMode = null, string MPHiddenInputMerchantAccountId = null);

        Task<string> ProcessMercadoPagoPixPayment(
            decimal amount, string clienteNome, string clienteEmail,
            string MPHiddenInputToken = null, string MPHiddenInputPaymentMethod = null, string MPHiddenInputProcessingMode = null, string MPHiddenInputMerchantAccountId = null);

        // Método para criar preferência no Mercado Pago
        Task<string> CreateMercadoPagoPreference(decimal amount, string description);
    }

}