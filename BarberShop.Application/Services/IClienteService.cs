using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<Agendamento>> ObterHistoricoAgendamentosAsync(int clienteId);
        Task<IEnumerable<Cliente>> ObterTodosClientesAsync();
        Task<Cliente> ObterClientePorIdAsync(int clienteId);
        Task AdicionarClienteAsync(Cliente cliente);
        Task AtualizarClienteAsync(Cliente cliente);
        Task DeletarClienteAsync(int clienteId);
        Task<Cliente> ObterClientePorEmailOuTelefoneAsync(string email, string telefone, int barbeariaId);
    }
}
