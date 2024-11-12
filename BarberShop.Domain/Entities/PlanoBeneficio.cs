using System;

namespace BarberShop.Domain.Entities
{
    public class PlanoBeneficio
    {
        public int PlanoBeneficioId { get; set; }
        public int PlanoBarbeariaId { get; set; }
        public int ServicoId { get; set; }
        public int Quantidade { get; set; }

        public PlanoAssinaturaBarbearia PlanoAssinaturaBarbearia { get; set; }
        public Servico Servico { get; set; }
    }
}
