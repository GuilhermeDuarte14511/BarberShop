using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class PagamentoAssinatura
    {
        public int AssinaturaId { get; set; } // Primary key
        public int ClienteId { get; set; }
        public string NomeCliente { get; set; }
        public string EmailCliente { get; set; }
        public string TelefoneCliente { get; set; }
        public decimal ValorPago { get; set; }
        public string PaymentId { get; set; }
        public string StatusPagamento { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public int BarbeariaId { get; set; }
    }

}
