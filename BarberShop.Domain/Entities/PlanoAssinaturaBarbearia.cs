using System;

namespace BarberShop.Domain.Entities
{
    public class PlanoAssinaturaBarbearia
    {
        public int PlanoBarbeariaId { get; set; }
        public int BarbeariaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public string Periodicidade { get; set; }

        public Barbearia Barbearia { get; set; }
    }
}
