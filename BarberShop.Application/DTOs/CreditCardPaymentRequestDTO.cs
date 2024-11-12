using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class CreditCardPaymentRequestDTO
    {
        public decimal Amount { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteEmail { get; set; }
    }
}
