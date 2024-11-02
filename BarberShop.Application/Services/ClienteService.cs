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

        public async Task<IEnumerable<Agendamento>> ObterHistoricoAgendamentosAsync(int clienteId)
        {
            // Busca o histórico de agendamentos do cliente
            var agendamentos = await _agendamentoRepository.GetByClienteIdWithServicosAsync(clienteId) as IEnumerable<Agendamento>;

            // Verifique se a conversão foi bem-sucedida
            if (agendamentos == null)
            {
                throw new InvalidCastException("O resultado de GetByClienteIdWithServicosAsync não pôde ser convertido para IEnumerable<Agendamento>.");
            }

            // Ordenar os agendamentos do mais recente para o mais antigo
            return agendamentos.OrderByDescending(a => a.AgendamentoId);
        }



        public async Task<IEnumerable<Cliente>> ObterTodosClientesAsync()
        {
            // Busca todos os clientes
            return await _clienteRepository.GetAllAsync();
        }

        public async Task<Cliente> ObterClientePorIdAsync(int clienteId)
        {
            // Busca um cliente pelo ID
            return await _clienteRepository.GetByIdAsync(clienteId);
        }

        public async Task AdicionarClienteAsync(Cliente cliente)
        {
            // Adiciona um novo cliente
            await _clienteRepository.AddAsync(cliente);
        }

        public async Task AtualizarClienteAsync(Cliente cliente)
        {
            // Atualiza um cliente existente
            await _clienteRepository.UpdateAsync(cliente);
        }

        public async Task DeletarClienteAsync(int clienteId)
        {
            // Deleta um cliente
            await _clienteRepository.DeleteAsync(clienteId);
        }

        public async Task<Cliente> ObterClientePorEmailOuTelefoneAsync(string emailOuTelefone)
        {
            // Busca um cliente por email ou telefone
            return await _clienteRepository.GetByEmailOrPhoneAsync(emailOuTelefone);
        }
    }
}
