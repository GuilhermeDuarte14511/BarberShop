using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class ServicoRepository : IServicoRepository
    {
        private readonly BarbeariaContext _context;

        public ServicoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<Servico> AddAsync(Servico entity)
        {
            await _context.Servicos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico != null)
            {
                _context.Servicos.Remove(servico);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Servico>> GetAllAsync()
        {
            return await _context.Servicos.ToListAsync();
        }

        public async Task<Servico> GetByIdAsync(int id)
        {
            return await _context.Servicos.FindAsync(id);
        }

        public async Task UpdateAsync(Servico entity)
        {
            _context.Servicos.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Implementação do método para buscar múltiplos serviços por uma lista de IDs
        public async Task<IEnumerable<Servico>> ObterServicosPorIdsAsync(IEnumerable<int> servicoIds)
        {
            return await _context.Servicos.Where(s => servicoIds.Contains(s.ServicoId)).ToListAsync();
        }
    }
}
