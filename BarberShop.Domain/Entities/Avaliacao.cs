using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class Avaliacao
    {
        public int AvaliacaoId { get; set; }
        public int AgendamentoId { get; set; }
        public string Observacao { get; set; }
        public int Nota { get; set; }
        public Agendamento Agendamento { get; set; }
    }

}
