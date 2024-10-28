using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IAgendamentoRepository : IRepository<Agendamento>
    {
        Task<IEnumerable<Agendamento>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime date, int duracaoTotal);
        Task<IEnumerable<Agendamento>> GetByClienteIdWithServicosAsync(int clienteId);
        Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroIdAsync(int barbeiroId, DateTime data);
    }
}
