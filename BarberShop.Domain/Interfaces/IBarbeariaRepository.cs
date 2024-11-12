using System.Threading.Tasks;
using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IBarbeariaRepository : IRepository<Barbearia>
    {
        Task<Barbearia> GetByUrlSlugAsync(string urlSlug);
        Task<IEnumerable<Barbearia>> ObterTodasAtivasAsync();
        Task DeleteAsync(Barbearia barbearia);

        // Novo método para verificar existência de UrlSlug
        Task<bool> ExistsByUrlSlugAsync(string urlSlug);
    }
}
