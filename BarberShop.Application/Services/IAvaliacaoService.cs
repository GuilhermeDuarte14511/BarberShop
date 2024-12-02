using BarberShop.Domain.Entities;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IAvaliacaoService
    {
        Task<Avaliacao> AdicionarAvaliacaoAsync(Avaliacao avaliacao);
        Task<Agendamento> ObterAgendamentoPorIdAsync(int agendamentoId); // Método ausente
        Task<Avaliacao> ObterAvaliacaoPorAgendamentoIdAsync(int agendamentoId); // Novo método
        Task<IEnumerable<Avaliacao>> ObterAvaliacoesPorBarbeiroIdAsync(int barbeiroId, int? avaliacaoId = null);
        Task<IEnumerable<Avaliacao>> ObterAvaliacoesFiltradasAsync(int? barbeariaId = null, int? barbeiroId = null, string? dataInicio = null, string? dataFim = null, int? notaServico = null, int? notaBarbeiro = null, string? observacao = null);

    }
}
