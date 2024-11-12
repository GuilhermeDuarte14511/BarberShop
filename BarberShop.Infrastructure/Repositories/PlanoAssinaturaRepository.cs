using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class PlanoAssinaturaRepository : IPlanoAssinaturaRepository
    {
        private readonly BarbeariaContext _context;

        public PlanoAssinaturaRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<PlanoAssinaturaSistema> AddAsync(PlanoAssinaturaSistema entity)
        {
            _context.PlanoAssinaturaSistema.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var plano = await GetByIdAsync(id);
            if (plano != null)
            {
                _context.PlanoAssinaturaSistema.Remove(plano);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PlanoAssinaturaSistema>> GetAllAsync()
        {
            return await _context.PlanoAssinaturaSistema.ToListAsync();
        }

        public async Task<PlanoAssinaturaSistema> GetByIdAsync(int id)
        {
            return await _context.PlanoAssinaturaSistema.FindAsync(id);
        }

        public async Task<List<PlanoAssinaturaSistema>> GetAllPlanosAsync()
        {
            return await _context.PlanoAssinaturaSistema.ToListAsync();
        }

        public async Task<PlanoAssinaturaSistema> GetByStripeIdAsync(string idProdutoStripe)
        {
            return await _context.PlanoAssinaturaSistema
                .FirstOrDefaultAsync(plano => plano.IdProdutoStripe == idProdutoStripe);
        }

        public async Task UpdateAsync(PlanoAssinaturaSistema entity)
        {
            _context.PlanoAssinaturaSistema.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
