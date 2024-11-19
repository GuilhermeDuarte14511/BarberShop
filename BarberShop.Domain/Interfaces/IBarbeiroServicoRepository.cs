using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IBarbeiroServicoRepository : IRepository<BarbeiroServico>
    {
        Task<IEnumerable<Servico>> ObterServicosPorBarbeiroIdAsync(int barbeiroId);
        Task<IEnumerable<Servico>> ObterServicosNaoVinculadosAsync(int barbeiroId);
        Task<bool> DesvincularServicoAsync(int barbeiroId, int servicoId);
        Task VincularServicoAsync(int barbeiroId, int servicoId);
        Task<Servico> ObterServicoPorIdAsync(int servicoId);

    }
}
