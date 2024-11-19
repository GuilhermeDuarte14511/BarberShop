using System;

namespace BarberShop.Domain.Entities
{
    public class BarbeiroServico
    {
        public int BarbeiroId { get; set; }
        public int ServicoId { get; set; }

        // Relacionamentos com as entidades Barbeiro e Servico
        public Barbeiro Barbeiro { get; set; }
        public Servico Servico { get; set; }

        public BarbeiroServico()
        {
        }
    }
}
