using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int[]> GetAgendamentosPorSemanaAsync(int barbeariaId);
        Task<Dictionary<string, int>> GetServicosMaisSolicitadosAsync(int barbeariaId);
        Task<Dictionary<string, decimal>> GetLucroPorBarbeiroAsync(int barbeariaId);
        Task<Dictionary<string, int>> GetAtendimentosPorBarbeiroAsync(int barbeariaId);
        Task<decimal[]> GetLucroDaSemanaAsync(int barbeariaId);
        Task<decimal[]> GetLucroDoMesAsync(int barbeariaId);
        Task<Dictionary<string, decimal>> GetCustomReportDataAsync(int barbeariaId, string reportType, int periodDays);
        Task SaveChartPositionsAsync(List<GraficoPosicao> posicoes);
        Task<List<GraficoPosicao>> GetChartPositionsAsync(int usuarioId);
    }
}
