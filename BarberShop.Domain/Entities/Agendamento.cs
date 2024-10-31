using System;
using System.Collections.Generic;

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

        // Novo campo para status do pagamento
        public StatusPagamento StatusPagamento { get; set; } = StatusPagamento.Pendente;
        public string? PaymentId { get; set; }


        // Relacionamento com Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int BarbeiroId { get; set; }
        public Barbeiro Barbeiro { get; set; }

        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; }
    }
}
