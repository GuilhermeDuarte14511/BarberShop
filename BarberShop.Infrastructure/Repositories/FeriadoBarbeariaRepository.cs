using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class FeriadoBarbeariaRepository : Repository<FeriadoBarbearia>, IFeriadoBarbeariaRepository
    {
        private readonly BarbeariaContext _context;

        public FeriadoBarbeariaRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeriadoBarbearia>> ObterFeriadosPorBarbeariaAsync(int barbeariaId)
        {
            return await _context.FeriadosBarbearias
                .Where(f => f.BarbeariaId == barbeariaId)
                .ToListAsync();
        }
    }
}
