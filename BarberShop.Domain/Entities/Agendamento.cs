using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BarberShop.Domain.Entities
{
    public class Agendamento
    {
        public int AgendamentoId { get; set; }
        public DateTime DataHora { get; set; }
        public StatusAgendamento Status { get; set; }
        public int? DuracaoTotal { get; set; }
        public string? FormaPagamento { get; set; }
        public decimal? PrecoTotal { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int BarbeiroId { get; set; }
        public Barbeiro Barbeiro { get; set; }

        public int BarbeariaId { get; set; }
        public Barbearia Barbearia { get; set; }

        public bool? EmailEnviado { get; set; } // Coluna de controle para e-mail

        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; }

        public Pagamento Pagamento { get; set; }

        [JsonIgnore] // Ignora a serialização dessa propriedade para evitar ciclos
        public ICollection<Avaliacao> Avaliacoes { get; set; }
    }
}
