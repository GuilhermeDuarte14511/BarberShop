using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IPagamentoRepository : IRepository<Pagamento>
    {
        Task<IEnumerable<Pagamento>> GetPagamentosPorAgendamentoIdAsync(int agendamentoId);
        Task<IEnumerable<Pagamento>> GetAllPagamentosByBarbeariaIdAsync(int barbeariaId);
    }
}
