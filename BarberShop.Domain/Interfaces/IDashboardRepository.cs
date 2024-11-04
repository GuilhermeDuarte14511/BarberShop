using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int[]> GetAgendamentosPorSemanaAsync();
        Task<Dictionary<string, int>> GetServicosMaisSolicitadosAsync();
        Task<Dictionary<string, decimal>> GetLucroPorBarbeiroAsync();
        Task<Dictionary<string, int>> GetAtendimentosPorBarbeiroAsync();
        Task<decimal[]> GetLucroDaSemanaAsync();
        Task<decimal[]> GetLucroDoMesAsync();
    }

}
