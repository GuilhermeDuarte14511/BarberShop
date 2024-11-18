using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IFeriadoBarbeariaService
    {
        Task<IEnumerable<FeriadoBarbearia>> ObterFeriadosPorBarbeariaAsync(int barbeariaId);
        Task<IEnumerable<FeriadoNacional>> ObterFeriadosNacionaisAsync();
        Task<FeriadoBarbearia> ObterPorIdAsync(int id);
        Task AdicionarFeriadoAsync(FeriadoBarbearia feriado);
        Task AtualizarFeriadoAsync(FeriadoBarbearia feriado);
        Task ExcluirFeriadoAsync(int id);
    }
}
