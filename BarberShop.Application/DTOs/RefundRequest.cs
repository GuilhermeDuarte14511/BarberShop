using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class RefundRequest
    {
        public string PaymentId { get; set; }
        public long? Amount { get; set; } // Valor opcional para reembolso parcial em centavos
    }
}
