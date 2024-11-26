using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;

namespace BarberShop.Application.DTOs
{
    public class AgendamentoDto
    {
        public int? AgendamentoId { get; set; }
        public DateTime? DataHora { get; set; }
        public StatusAgendamento Status { get; set; }
        public int? DuracaoTotal { get; set; }
        public string FormaPagamento { get; set; }
        public decimal? PrecoTotal { get; set; }

        // Dados do Cliente (pode ser omitido se não for necessário na view)
        public ClienteDTO? Cliente { get; set; }

        // Dados do Barbeiro
        public string? BarbeiroNome { get; set; } // Simplifica para apenas o nome do barbeiro

        // Dados do Pagamento
        public string? StatusPagamento { get; set; } // Armazena o status do pagamento como string

        // Lista de serviços associados ao agendamento
        public List<ServicoDTO>? Servicos { get; set; }
        public Pagamento? Pagamento { get; set; }
    }
}
