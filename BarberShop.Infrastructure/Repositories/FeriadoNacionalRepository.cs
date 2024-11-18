using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class FeriadoNacionalRepository : Repository<FeriadoNacional>, IFeriadoNacionalRepository
    {
        private readonly BarbeariaContext _context;

        public FeriadoNacionalRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeriadoNacional>> ObterTodosFeriadosAsync()
        {
            return await _context.FeriadosNacionais.ToListAsync();
        }
    }
}
