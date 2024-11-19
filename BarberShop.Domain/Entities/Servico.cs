using System.Collections.Generic;

namespace BarberShop.Domain.Entities
{
    public class Servico
    {
        public int ServicoId { get; set; }
        public string Nome { get; set; }
        public float Preco { get; set; }
        public int Duracao { get; set; } // Em minutos

        public int? BarbeariaId { get; set; }
        public Barbearia? Barbearia { get; set; }

        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; }

        // Relacionamento com BarbeiroServico
        public ICollection<BarbeiroServico> BarbeiroServicos { get; set; }

        public Servico()
        {
            AgendamentoServicos = new List<AgendamentoServico>();
            BarbeiroServicos = new List<BarbeiroServico>();
        }
    }
}
