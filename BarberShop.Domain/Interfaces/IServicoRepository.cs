using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IServicoRepository : IRepository<Servico>
    {
        Task<IEnumerable<Servico>> ObterServicosPorIdsAsync(IEnumerable<int> servicoIds);
        Task<IEnumerable<Servico>> ObterServicosPorBarbeariaIdAsync(int barbeariaId);
        Task<IEnumerable<string>> ObterNomesServicosAsync(IEnumerable<int> servicoIds);

    }
}
