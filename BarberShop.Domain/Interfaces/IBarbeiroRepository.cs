using BarberShop.Domain.Entities;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IBarbeiroRepository : IRepository<Barbeiro>
    {
        Task<Barbeiro> GetByEmailOrPhoneAsync(string email, string telefone);
        Task<IEnumerable<Barbeiro>> GetAllByBarbeariaIdAsync(int barbeariaId);
        Task<IEnumerable<(int BarbeiroId, int ServicoId)>> ObterBarbeirosComServicosPorBarbeariaIdAsync(int barbeariaId);

        Task<IEnumerable<Servico>> ObterServicosPorBarbeiroIdAsync(int barbeiroId);
        Task<Barbeiro> CriarBarbeiroAsync(Barbeiro barbeiro);
        Task DeleteAsync(int id);


    }
}
