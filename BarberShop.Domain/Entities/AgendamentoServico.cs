using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarberShop.Domain.Entities
{
    public class AgendamentoServico
    {
        [Key, Column(Order = 0)]
        public int AgendamentoId { get; set; }
        public Agendamento? Agendamento { get; set; }

        [Key, Column(Order = 1)]
        public int ServicoId { get; set; }
        public Servico? Servico { get; set; }
    }
}
