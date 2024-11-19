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

    }
}
