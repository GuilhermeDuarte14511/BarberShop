using System;

namespace BarberShop.Domain.Entities
{
    public class Avaliacao
    {
        public int AvaliacaoId { get; set; }
        public int AgendamentoId { get; set; }
        public string Observacao { get; set; }
        public int Nota { get; set; }

        public int BarbeariaId { get; set; }
        public Barbearia Barbearia { get; set; }

        public Agendamento Agendamento { get; set; }
    }
}
