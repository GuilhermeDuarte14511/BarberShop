using System.Collections.Generic;
using System.Threading.Tasks;
using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IRelatorioPersonalizadoRepository
    {
        Task SalvarRelatorioPersonalizadoAsync(RelatorioPersonalizado relatorio);
        Task<List<RelatorioPersonalizado>> ObterRelatoriosPorUsuarioAsync(int usuarioId);
        Task<RelatorioPersonalizado> ObterRelatorioPorIdAsync(int id);
        Task AtualizarRelatorioAsync(RelatorioPersonalizado relatorio);
        Task DeletarRelatorioAsync(int id);
    }
}
