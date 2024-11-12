using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente> GetByIdAndBarbeariaIdAsync(int clienteId, int barbeariaId);
        Task<IEnumerable<Cliente>> GetAllByBarbeariaIdAsync(int barbeariaId);
        Task<Cliente> GetByEmailOrPhoneAsync(string email, string phone, int barbeariaId);
        Task UpdateCodigoVerificacaoAsync(int clienteId, string codigoVerificacao, DateTime? expiracao);

    }
}
