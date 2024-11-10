using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IServicoService
    {
        Task<IEnumerable<Servico>> ObterTodosPorBarbeariaIdAsync(int barbeariaId);
        Task<Servico> ObterPorIdAsync(int id);
        Task AdicionarAsync(Servico servico);
        Task AtualizarAsync(Servico servico);
        Task ExcluirAsync(int id);
    }
}
