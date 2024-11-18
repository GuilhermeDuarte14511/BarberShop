using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class IndisponibilidadeRepository : Repository<IndisponibilidadeBarbeiro>, IIndisponibilidadeRepository
    {
        private readonly BarbeariaContext _context;

        public IndisponibilidadeRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        // Retorna apenas datas de início e fim
        public async Task<IEnumerable<(DateTime DataInicio, DateTime DataFim)>> ObterDatasIndisponibilidadesPorBarbeiroAsync(int barbeiroId)
        {
            return await _context.IndisponibilidadesBarbeiros
                .Where(i => i.BarbeiroId == barbeiroId)
                .Select(i => new { i.DataInicio, i.DataFim })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.DataInicio, x.DataFim)));
        }

        // Retorna entidades completas de indisponibilidade
        public async Task<IEnumerable<IndisponibilidadeBarbeiro>> ObterIndisponibilidadesPorBarbeiroAsync(int barbeiroId)
        {
            return await _context.IndisponibilidadesBarbeiros
                .Where(i => i.BarbeiroId == barbeiroId)
                .Include(i => i.Barbeiro)
                .ToListAsync();
        }

        // Retorna todas as indisponibilidades de uma barbearia
        public async Task<IEnumerable<IndisponibilidadeBarbeiro>> GetByBarbeariaIdAsync(int barbeariaId)
        {
            return await _context.IndisponibilidadesBarbeiros
                .Where(i => i.Barbeiro.BarbeariaId == barbeariaId)
                .Include(i => i.Barbeiro)
                .ToListAsync();
        }
    }
}
