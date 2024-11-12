using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class SavePaymentRequestDTO
    {
        public int ClienteId { get; set; }
        public string NomeCliente { get; set; }
        public string EmailCliente { get; set; }
        public string TelefoneCliente { get; set; }
        public decimal ValorPago { get; set; }
        public string PaymentId { get; set; }
        public string StatusPagamento { get; set; }
        public int BarbeariaId { get; set; }
    }

}
