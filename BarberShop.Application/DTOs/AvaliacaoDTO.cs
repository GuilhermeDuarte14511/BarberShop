using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class AvaliacaoDTO
    {
        public int AvaliacaoId { get; set; }
        public int AgendamentoId { get; set; }
        public int NotaBarbeiro { get; set; }
        public int NotaServico { get; set; }
        public string Observacao { get; set; }
        public DateTime DataAvaliado { get; set; }
        public string? BarbeiroNome { get; set; }
        public string? ClienteNome { get; set; }
        public string? ClienteEmail { get; set; }
    }

}
