using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IFeriadoBarbeariaRepository : IRepository<FeriadoBarbearia>
    {
        Task<IEnumerable<FeriadoBarbearia>> ObterFeriadosPorBarbeariaAsync(int barbeariaId);
    }
}
