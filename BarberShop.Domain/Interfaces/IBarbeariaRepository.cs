using System.Threading.Tasks;
using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IBarbeariaRepository : IRepository<Barbearia>
    {
        Task<Barbearia> GetByUrlSlugAsync(string urlSlug);
    }
}
