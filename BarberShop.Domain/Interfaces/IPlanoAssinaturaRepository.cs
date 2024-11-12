using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IPlanoAssinaturaRepository : IRepository<PlanoAssinaturaSistema>
    {
        Task<List<PlanoAssinaturaSistema>> GetAllPlanosAsync();
        Task<PlanoAssinaturaSistema> GetByStripeIdAsync(string stripeId);

    }
}
