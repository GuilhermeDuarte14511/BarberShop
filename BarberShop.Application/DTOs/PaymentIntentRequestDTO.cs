using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class PaymentIntentRequestDTO
    {
        public decimal Amount { get; set; }
        public List<string> PaymentMethods { get; set; } = new List<string> { "card" }; // Default to "card"
        public string Currency { get; set; } = "brl";
    }
}
