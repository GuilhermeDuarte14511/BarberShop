using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IIndisponibilidadeRepository : IRepository<IndisponibilidadeBarbeiro>
    {
        Task<IEnumerable<(DateTime DataInicio, DateTime DataFim)>> ObterDatasIndisponibilidadesPorBarbeiroAsync(int barbeiroId); // Retorna apenas datas
        Task<IEnumerable<IndisponibilidadeBarbeiro>> ObterIndisponibilidadesPorBarbeiroAsync(int barbeiroId); // Retorna entidades completas
        Task<IEnumerable<IndisponibilidadeBarbeiro>> GetByBarbeariaIdAsync(int barbeariaId); // Indisponibilidades por barbearia
    }
}
