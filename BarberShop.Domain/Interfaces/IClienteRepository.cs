using BarberShop.Domain.Entities;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente> GetByEmailOrPhoneAsync(string emailOrPhone);
    }
}
