using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class BarbeiroRepository : IBarbeiroRepository
    {
        private readonly BarbeariaContext _context;

        public BarbeiroRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<Barbeiro> AddAsync(Barbeiro entity)
        {
            await _context.Barbeiros.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;  // Retorna o barbeiro adicionado
        }

        public async Task DeleteAsync(int id)
        {
            var barbeiro = await _context.Barbeiros.FindAsync(id);
            if (barbeiro != null)
            {
                _context.Barbeiros.Remove(barbeiro);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Barbeiro>> GetAllAsync()
        {
            return await _context.Barbeiros.ToListAsync();
        }

        public async Task<Barbeiro> GetByIdAsync(int id)
        {
            return await _context.Barbeiros.FindAsync(id);
        }

        public async Task UpdateAsync(Barbeiro entity)
        {
            _context.Barbeiros.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Barbeiro> GetByEmailOrPhoneAsync(string email, string telefone)
        {
            return await _context.Barbeiros
                .FirstOrDefaultAsync(b => b.Email == email || b.Telefone == telefone);
        }

        public async Task<IEnumerable<Barbeiro>> GetByBarbeariaIdAsync(int barbeariaId)
        {
            return await _context.Barbeiros
                .Where(b => b.BarbeariaId == barbeariaId)
                .ToListAsync();
        }

        // Implementação de GetAllByBarbeariaIdAsync
        public async Task<IEnumerable<Barbeiro>> GetAllByBarbeariaIdAsync(int barbeariaId)
        {
            return await _context.Barbeiros
                .Where(b => b.BarbeariaId == barbeariaId)
                .ToListAsync();
        }

        // Implementação do SaveChangesAsync
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<(int BarbeiroId, int ServicoId)>> ObterBarbeirosComServicosPorBarbeariaIdAsync(int barbeariaId)
        {
             var resultado = await _context.BarbeiroServicos
                .Where(bs => bs.Barbeiro.BarbeariaId == barbeariaId)
                .Select(bs => new { bs.BarbeiroId, bs.ServicoId }) // Usar tipo anônimo
                .ToListAsync();

            // Converter para tuplas
            return resultado.Select(r => (r.BarbeiroId, r.ServicoId));
        }

        public async Task<IEnumerable<Servico>> ObterServicosPorBarbeiroIdAsync(int barbeiroId)
        {
            return await _context.BarbeiroServicos
                .Where(bs => bs.BarbeiroId == barbeiroId)
                .Select(bs => bs.Servico)
                .ToListAsync();
        }

        public async Task<Barbeiro> CriarBarbeiroAsync(Barbeiro barbeiro)
        {
            await _context.Barbeiros.AddAsync(barbeiro);
            await _context.SaveChangesAsync();
            return barbeiro;
        }


    }
}
