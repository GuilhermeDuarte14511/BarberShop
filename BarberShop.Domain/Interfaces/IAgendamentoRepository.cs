using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IAgendamentoRepository : IRepository<Agendamento>
    {
        Task<IEnumerable<Agendamento>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime date, int duracaoTotal);
        Task<IEnumerable<Agendamento>> GetByClienteIdWithServicosAsync(int clienteId);
        Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroIdAsync(int barbeiroId, DateTime data);
        Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEHorarioAsync(int barbeiroId, DateTime dataHoraInicio, DateTime dataHoraFim);
        Task AtualizarStatusPagamentoAsync(int agendamentoId, StatusPagamento statusPagamento, string paymentId = null);
    }
}
