using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;

namespace BarberShop.Application.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IServicoRepository _servicoRepository;

        public AgendamentoService(IAgendamentoRepository agendamentoRepository, IServicoRepository servicoRepository)
        {
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository;
        }

        public async Task<Servico> CriarServicoAsync(Servico servico)
        {
            return await _servicoRepository.AddAsync(servico);
        }

        public async Task<Agendamento> ObterAgendamentoPorIdAsync(int id)
        {
            return await _agendamentoRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Servico>> ObterServicosAsync()
        {
            return await _servicoRepository.GetAllAsync();
        }

        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, DateTime data, int duracaoTotal)
        {
            return await _agendamentoRepository.GetAvailableSlotsAsync(barbeiroId, data, duracaoTotal);
        }

        // Método para criar um agendamento
        public async Task<int> CriarAgendamentoAsync(int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds)
        {
            // Obter os serviços com base nos IDs fornecidos
            var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIds);

            // Criar o novo agendamento
            var novoAgendamento = new Agendamento
            {
                BarbeiroId = barbeiroId,
                ClienteId = clienteId, // Adicionando o ClienteId aqui
                DataHora = dataHora,
                DuracaoTotal = servicos.Sum(s => s.Duracao), // Calcular a duração total com base nos serviços
                AgendamentoServicos = servicos.Select(s => new AgendamentoServico { ServicoId = s.ServicoId }).ToList() // Associar os serviços ao agendamento
            };

            // Salvar o novo agendamento no repositório e retornar o ID
            var agendamento = await _agendamentoRepository.AddAsync(novoAgendamento);
            return agendamento.AgendamentoId;
        }
    }
}
