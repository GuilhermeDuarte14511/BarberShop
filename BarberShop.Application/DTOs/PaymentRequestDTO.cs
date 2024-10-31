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
        public string CardholderEmail { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        public string CardholderName { get; set; } // Nome do titular do cartão
    }

}
