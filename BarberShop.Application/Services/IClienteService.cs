using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<Agendamento>> ObterHistoricoAgendamentosAsync(int clienteId, int? barbeariaId);
        Task<IEnumerable<Cliente>> ObterTodosClientesAsync(int barbeariaId);
        Task<Cliente> ObterClientePorIdAsync(int clienteId, int barbeariaId);
        Task AdicionarClienteAsync(Cliente cliente, int barbeariaId);
        Task AtualizarClienteAsync(Cliente cliente, int barbeariaId);
        Task DeletarClienteAsync(int clienteId, int barbeariaId);
        Task<Cliente> ObterClientePorEmailOuTelefoneAsync(string email, string telefone, int barbeariaId);
        Task AtualizarDadosClienteAsync(int clienteId, string nome, string email, string telefone, int barbeariaId);

    }
}
