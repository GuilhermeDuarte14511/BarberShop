using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente> GetByEmailOrPhoneAsync(string email, string telefone, int barbeariaId);
    Task UpdateCodigoVerificacaoAsync(int clienteId, string codigoVerificacao, DateTime? expiracao);
}
