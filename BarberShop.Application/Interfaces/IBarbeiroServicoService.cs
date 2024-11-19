using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IBarbeiroServicoService
    {
        Task<IEnumerable<Servico>> ObterServicosPorBarbeiroIdAsync(int barbeiroId);
        Task<IEnumerable<Servico>> ObterServicosNaoVinculadosAsync(int barbeiroId);
        Task<Servico> ObterServicoPorIdAsync(int servicoId); 
        Task VincularServicoAsync(int barbeiroId, int servicoId);
        Task<bool> DesvincularServicoAsync(int barbeiroId, int servicoId);
    }
}
