using System;
using System.Text.Json.Serialization;

namespace BarberShop.Domain.Entities
{
    public class Avaliacao
    {
        public int AvaliacaoId { get; set; }
        public int AgendamentoId { get; set; }

        [JsonIgnore] // Ignora a serialização dessa propriedade
        public Agendamento Agendamento { get; set; }

        public int NotaBarbeiro { get; set; }
        public int NotaServico { get; set; }
        public string Observacao { get; set; }

        // Define DataAvaliado com o valor padrão de DateTime.Now
        public DateTime DataAvaliado { get; set; } = DateTime.Now;
    }
}
