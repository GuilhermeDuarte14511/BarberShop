using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class BarbeiroServicoRepository : IBarbeiroServicoRepository
    {
        private readonly BarbeariaContext _context;

        public BarbeiroServicoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<BarbeiroServico> GetByIdAsync(int id)
        {
            return await _context.BarbeiroServicos.FindAsync(id);
        }

        public async Task<IEnumerable<BarbeiroServico>> GetAllAsync()
        {
            return await _context.BarbeiroServicos.ToListAsync();
        }

        public async Task<BarbeiroServico> AddAsync(BarbeiroServico entity)
        {
            await _context.BarbeiroServicos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(BarbeiroServico entity)
        {
            _context.BarbeiroServicos.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.BarbeiroServicos.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Servico>> ObterServicosPorBarbeiroIdAsync(int barbeiroId)
        {
            return await _context.Servicos
                .Where(s => _context.BarbeiroServicos.Any(bs => bs.BarbeiroId == barbeiroId && bs.ServicoId == s.ServicoId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Servico>> ObterServicosNaoVinculadosAsync(int barbeiroId)
        {
            var barbeariaId = await _context.Barbeiros
                .Where(b => b.BarbeiroId == barbeiroId)
                .Select(b => b.BarbeariaId)
                .FirstOrDefaultAsync();

            return await _context.Servicos
                .Where(s => s.BarbeariaId == barbeariaId &&
                            !_context.BarbeiroServicos.Any(bs => bs.BarbeiroId == barbeiroId && bs.ServicoId == s.ServicoId))
                .ToListAsync();
        }

        public async Task VincularServicoAsync(int barbeiroId, int servicoId)
        {
            var barbeiroServico = new BarbeiroServico
            {
                BarbeiroId = barbeiroId,
                ServicoId = servicoId
            };

            await _context.BarbeiroServicos.AddAsync(barbeiroServico);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DesvincularServicoAsync(int barbeiroId, int servicoId)
        {
            var barbeiroServico = await _context.BarbeiroServicos
                .FirstOrDefaultAsync(bs => bs.BarbeiroId == barbeiroId && bs.ServicoId == servicoId);

            if (barbeiroServico != null)
            {
                _context.BarbeiroServicos.Remove(barbeiroServico);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<Servico> ObterServicoPorIdAsync(int servicoId)
        {
            return await _context.Servicos.FirstOrDefaultAsync(s => s.ServicoId == servicoId);
        }

    }
}
