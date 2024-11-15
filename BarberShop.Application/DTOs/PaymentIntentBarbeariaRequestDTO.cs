using System;
using System.Collections.Generic;

namespace BarberShop.Application.DTOs
{
    public class PaymentIntentBarbeariaRequestDTO
    {
        public string? BarbeariaId { get; set; }

        public decimal Amount { get; set; }

        public List<string>? PaymentMethods { get; set; }

        public string? Currency { get; set; } = "brl";

        public decimal? CommissionPercentage { get; set; }
    }
}
