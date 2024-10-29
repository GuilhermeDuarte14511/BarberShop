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

        // Corrigido: AddAsync agora adiciona um Barbeiro
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
    }
}
