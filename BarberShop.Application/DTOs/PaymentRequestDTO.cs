using MercadoPago.Resource.Payment;
using System;

namespace BarberShop.Application.DTOs
{
    public class PaymentRequestDTO
    {
        public decimal TransactionAmount { get; set; }
        public string Token { get; set; }
        public string Description { get; set; }
        public int Installments { get; set; }
        public string PaymentMethodId { get; set; }
        public PaymentPayer Payer { get; set; }
    }

}
