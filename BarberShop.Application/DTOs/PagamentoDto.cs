using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class PagamentoDto
    {
        public int PagamentoId { get; set; }
        public decimal ValorPago { get; set; }
        public StatusPagamento StatusPagamento { get; set; }
        public string PaymentId { get; set; }
        public DateTime? DataPagamento { get; set; }
    }
}
