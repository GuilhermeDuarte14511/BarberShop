using BarberShop.Application.DTOs;

namespace BarberShop.Application.Interfaces
{
    public interface IRedefinirSenhaService
    {
        Task<bool> ValidarTokenAsync(int clienteId, string token);
        Task<bool> RedefinirSenhaAsync(RedefinirSenhaDto redefinirSenhaDto);
        Task<string> ObterUrlRedirecionamentoAsync(int clienteId);
    }
}
