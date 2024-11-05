using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class AvaliacaoDTO
    {
        public int AgendamentoId { get; set; }
        public DateTime DataHora { get; set; }
        public int Status { get; set; }
        public int DuracaoTotal { get; set; }
        public string FormaPagamento { get; set; }
        public decimal PrecoTotal { get; set; }
        public string ClienteNome { get; set; }
        public string BarbeiroNome { get; set; }
        public string Servicos { get; set; }
    }
}
