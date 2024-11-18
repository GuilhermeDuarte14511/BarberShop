using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IIndisponibilidadeService
    {
        Task<IEnumerable<IndisponibilidadeBarbeiro>> ObterIndisponibilidadesPorBarbeiroAsync(int barbeiroId);
        Task<IEnumerable<IndisponibilidadeBarbeiro>> ObterIndisponibilidadesPorBarbeariaAsync(int barbeariaId); // Novo método
        Task<IndisponibilidadeBarbeiro> ObterPorIdAsync(int id);
        Task AdicionarIndisponibilidadeAsync(IndisponibilidadeBarbeiro indisponibilidade);
        Task AtualizarIndisponibilidadeAsync(IndisponibilidadeBarbeiro indisponibilidade);
        Task ExcluirIndisponibilidadeAsync(int id);
    }
}
