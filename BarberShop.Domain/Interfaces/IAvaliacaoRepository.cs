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
        Task<IEnumerable<Avaliacao>> GetAvaliacoesPorAgendamentoIdAsync(int agendamentoId);
    }
}
