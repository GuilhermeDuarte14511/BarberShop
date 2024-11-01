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

        public async Task<int> CriarAgendamentoAsync(int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds, string formaPagamento, decimal precoTotal, StatusPagamento statusPagamento = StatusPagamento.Pendente, string paymentId = null)
        {
            var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIds);
            var duracaoTotal = servicos.Sum(s => s.Duracao);

            var novoAgendamento = new Agendamento
            {
                BarbeiroId = barbeiroId,
                ClienteId = clienteId,
                DataHora = dataHora,
                DuracaoTotal = duracaoTotal,
                FormaPagamento = formaPagamento,
                PrecoTotal = precoTotal,
                StatusPagamento = statusPagamento,
                PaymentId = paymentId,
                AgendamentoServicos = servicos.Select(s => new AgendamentoServico { ServicoId = s.ServicoId }).ToList()
            };

            var agendamento = await _agendamentoRepository.AddAsync(novoAgendamento);
            return agendamento.AgendamentoId;
        }

        public async Task AtualizarStatusPagamentoAsync(int agendamentoId, StatusPagamento statusPagamento, string paymentId = null)
        {
            await _agendamentoRepository.AtualizarStatusPagamentoAsync(agendamentoId, statusPagamento, paymentId);
        }


    }
}
