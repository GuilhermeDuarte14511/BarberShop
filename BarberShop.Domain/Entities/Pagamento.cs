using System;

namespace BarberShop.Domain.Entities
{
    public class Pagamento
    {
        public int? PagamentoId { get; set; }
        public int? AgendamentoId { get; set; }
        public int? ClienteId { get; set; }
        public decimal? ValorPago { get; set; }
        public StatusPagamento StatusPagamento { get; set; }
        public string? PaymentId { get; set; }
        public DateTime? DataPagamento { get; set; }

        public Agendamento? Agendamento { get; set; }
        public Cliente? Cliente { get; set; }

        public int? BarbeariaId { get; set; }
        public Barbearia? Barbearia { get; set; }
    }
}
