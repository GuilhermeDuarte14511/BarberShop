using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class Servico
    {
        public int ServicoId { get; set; }
        public string Nome { get; set; }
        public float Preco { get; set; }
        public int Duracao { get; set; } // Em minutos

        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; } // Relacionamento com AgendamentoServico

    }
}
