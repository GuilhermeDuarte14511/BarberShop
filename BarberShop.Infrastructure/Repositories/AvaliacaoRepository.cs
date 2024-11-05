using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BarberShop.Infrastructure.Repositories
{
    public class AvaliacaoRepository : IAvaliacaoRepository
    {
        private readonly BarbeariaContext _context;

        public AvaliacaoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<Avaliacao> AddAsync(Avaliacao entity)
        {
            await _context.Avaliacao.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var avaliacao = await _context.Avaliacao.FindAsync(id);
            if (avaliacao != null)
            {
                _context.Avaliacao.Remove(avaliacao);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Avaliacao>> GetAllAsync()
        {
            return await _context.Avaliacao.ToListAsync();
        }

        public async Task<Avaliacao> GetByIdAsync(int id)
        {
            return await _context.Avaliacao.FindAsync(id);
        }

        public async Task<IEnumerable<Avaliacao>> GetAvaliacoesPorAgendamentoIdAsync(int agendamentoId)
        {
            return await _context.Avaliacao
                .Where(a => a.AgendamentoId == agendamentoId)
                .ToListAsync();
        }

        public async Task UpdateAsync(Avaliacao entity)
        {
            _context.Avaliacao.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
