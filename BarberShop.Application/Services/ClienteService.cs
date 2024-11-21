using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;

        public ClienteService(IClienteRepository clienteRepository, IAgendamentoRepository agendamentoRepository)
        {
            _clienteRepository = clienteRepository;
            _agendamentoRepository = agendamentoRepository;
        }

        public async Task<IEnumerable<Agendamento>> ObterHistoricoAgendamentosAsync(int clienteId, int? barbeariaId)
        {
            return await _agendamentoRepository.GetByClienteIdWithServicosAsync(clienteId, barbeariaId);
        }

        public async Task<IEnumerable<Cliente>> ObterTodosClientesAsync(int barbeariaId)
        {
            return await _clienteRepository.GetAllByBarbeariaIdAsync(barbeariaId);
        }

        public async Task<Cliente> ObterClientePorIdAsync(int clienteId, int barbeariaId)
        {
            return await _clienteRepository.GetByIdAndBarbeariaIdAsync(clienteId, barbeariaId);
        }

        public async Task AdicionarClienteAsync(Cliente cliente, int barbeariaId)
        {
            cliente.BarbeariaId = barbeariaId;
            await _clienteRepository.AddAsync(cliente);
        }

        public async Task AtualizarClienteAsync(Cliente cliente, int barbeariaId)
        {
            var clienteExistente = await _clienteRepository.GetByIdAndBarbeariaIdAsync(cliente.ClienteId, barbeariaId);
            if (clienteExistente != null)
            {
                cliente.BarbeariaId = barbeariaId;
                await _clienteRepository.UpdateAsync(cliente);
            }
        }

        public async Task DeletarClienteAsync(int clienteId, int barbeariaId)
        {
            var clienteExistente = await _clienteRepository.GetByIdAndBarbeariaIdAsync(clienteId, barbeariaId);
            if (clienteExistente != null)
            {
                await _clienteRepository.DeleteAsync(clienteId);
            }
        }

        public async Task<Cliente> ObterClientePorEmailOuTelefoneAsync(string email, string telefone, int barbeariaId)
        {
            return await _clienteRepository.GetByEmailOrPhoneAsync(email, telefone, barbeariaId);
        }

        public async Task AtualizarDadosClienteAsync(int clienteId, string nome, string email, string telefone, int barbeariaId)
        {
            await _clienteRepository.AtualizarDadosClienteAsync(clienteId, nome, email, telefone, barbeariaId);
        }

    }
}
