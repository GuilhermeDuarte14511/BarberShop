using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IAvaliacaoRepository : IRepository<Avaliacao>
    {
        Task<Avaliacao> ObterAvaliacaoPorAgendamentoIdAsync(int agendamentoId);
        Task<Avaliacao> AdicionarAvaliacaoAsync(Avaliacao avaliacao);
        Task<IEnumerable<Avaliacao>> ObterAvaliacoesPorBarbeiroIdAsync(int barbeiroId, int? avaliacaoId = null);
        Task<IEnumerable<Avaliacao>> ObterAvaliacoesFiltradasAsync(int? barbeariaId = null, int? barbeiroId = null, string? dataInicio = null, string? dataFim = null, int? notaServico = null, int? notaBarbeiro = null, string? observacao = null);

    }
}
