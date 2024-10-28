using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class Agendamento
    {
        public int AgendamentoId { get; set; } // Alterado para AgendamentoId
        public DateTime DataHora { get; set; }
        public StatusAgendamento Status { get; set; }
        public int DuracaoTotal { get; set; } // Adicionada para armazenar a duração total do agendamento


        // Relacionamento com Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        // Relacionamento com Barbeiro
        public int BarbeiroId { get; set; }
        public Barbeiro Barbeiro { get; set; }
        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; } // Adicionado relacionamento com AgendamentoServico
    }
}
