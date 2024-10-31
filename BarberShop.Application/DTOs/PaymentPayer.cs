using MercadoPago.Resource.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    // DTO para Payer
    public class PaymentPayer
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public Identification Identification { get; set; }
    }
}
