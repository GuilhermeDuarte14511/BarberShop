using BarberShop.Domain.Entities;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IAvaliacaoService
    {
        Task<Avaliacao> AdicionarAvaliacaoAsync(Avaliacao avaliacao);
        Task<Agendamento> ObterAgendamentoPorIdAsync(int agendamentoId); // Método ausente
        Task<Avaliacao> ObterAvaliacaoPorAgendamentoIdAsync(int agendamentoId); // Novo método

    }
}
