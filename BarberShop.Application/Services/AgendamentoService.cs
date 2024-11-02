using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;

namespace BarberShop.Application.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IPagamentoRepository _pagamentoRepository;

        public AgendamentoService(IAgendamentoRepository agendamentoRepository, IServicoRepository servicoRepository, IPagamentoRepository pagamentoRepository)
        {
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository;
            _pagamentoRepository = pagamentoRepository;
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

        public async Task<int> CriarAgendamentoAsync(int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds, string formaPagamento, decimal precoTotal)
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
                AgendamentoServicos = servicos.Select(s => new AgendamentoServico { ServicoId = s.ServicoId }).ToList()
            };

            var agendamento = await _agendamentoRepository.AddAsync(novoAgendamento);

            // Criar um pagamento associado ao agendamento
            var pagamento = new Pagamento
            {
                AgendamentoId = agendamento.AgendamentoId,
                ClienteId = clienteId,
                ValorPago = precoTotal,
                StatusPagamento = StatusPagamento.Pendente
            };

            await _pagamentoRepository.AddAsync(pagamento);

            return agendamento.AgendamentoId;
        }

        public async Task AtualizarStatusPagamentoAsync(int pagamentoId, StatusPagamento statusPagamento)
        {
            var pagamento = await _pagamentoRepository.GetByIdAsync(pagamentoId);
            if (pagamento != null)
            {
                pagamento.StatusPagamento = statusPagamento;
                await _pagamentoRepository.UpdateAsync(pagamento);
            }
        }

        public async Task<IEnumerable<Pagamento>> ObterPagamentosPorAgendamentoIdAsync(int agendamentoId)
        {
            return await _pagamentoRepository.GetPagamentosPorAgendamentoIdAsync(agendamentoId);
        }
    }
}
