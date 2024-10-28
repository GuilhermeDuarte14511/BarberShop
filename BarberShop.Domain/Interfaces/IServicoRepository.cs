using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IServicoRepository : IRepository<Servico>
    {
        Task<IEnumerable<Servico>> ObterServicosPorIdsAsync(IEnumerable<int> servicoIds);
    }
}
