using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int[]> GetAgendamentosPorSemanaAsync(int barbeariaId, int? barbeiroId = null);
        Task<Dictionary<string, int>> GetServicosMaisSolicitadosAsync(int barbeariaId, int? barbeiroId = null);
        Task<Dictionary<string, decimal>> GetLucroPorBarbeiroAsync(int barbeariaId, int? barbeiroId = null);
        Task<Dictionary<string, int>> GetAtendimentosPorBarbeiroAsync(int barbeariaId, int? barbeiroId = null);
        Task<decimal[]> GetLucroDaSemanaAsync(int barbeariaId, int? barbeiroId = null);
        Task<Dictionary<string, decimal>> GetLucroDoMesAsync(int barbeariaId, int? barbeiroId = null);
        Task<Dictionary<string, decimal>> GetCustomReportDataAsync(int barbeariaId, string reportType, int periodDays, int? barbeiroId = null);
        Task SaveChartPositionsAsync(List<GraficoPosicao> posicoes);
        Task<List<GraficoPosicao>> GetChartPositionsAsync(int usuarioId);
    }
}
