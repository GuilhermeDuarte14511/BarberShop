using System;
using System.Threading.Tasks;
using BarberShop.Domain.Entities;

namespace BarberShop.Domain.Interfaces
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> GetByEmailAsync(string email);
        Task UpdateCodigoVerificacaoAsync(int usuarioId, string codigoVerificacao, DateTime? expiracao);
    }
}
