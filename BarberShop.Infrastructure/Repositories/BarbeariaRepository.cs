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
    }
}
