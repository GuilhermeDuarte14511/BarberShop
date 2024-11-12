using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infrastructure.Repositories
{
    public class BarbeariaRepository : Repository<Barbearia>, IBarbeariaRepository
    {
        private readonly BarbeariaContext _context;

        public BarbeariaRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Barbearia> GetByUrlSlugAsync(string urlSlug)
        {
            return await _context.Barbearias.FirstOrDefaultAsync(b => b.UrlSlug == urlSlug);
        }

        public async Task<IEnumerable<Barbearia>> ObterTodasAtivasAsync()
        {
            return await _context.Barbearias
                .Where(b => b.Status == true)
                .ToListAsync();
        }

        public async Task DeleteAsync(Barbearia barbearia)
        {
            _context.Barbearias.Remove(barbearia);
            await _context.SaveChangesAsync();
        }

        // Implementação do novo método para verificar existência de UrlSlug
        public async Task<bool> ExistsByUrlSlugAsync(string urlSlug)
        {
            return await _context.Barbearias.AnyAsync(b => b.UrlSlug == urlSlug);
        }
    }
}
