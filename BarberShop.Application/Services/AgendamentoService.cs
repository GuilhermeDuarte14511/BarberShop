using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, DateTime data, int duracaoTotal)
        {
            return await _agendamentoRepository.GetAvailableSlotsAsync(barbeiroId, data, duracaoTotal);
        }

        public async Task<Agendamento> ObterAgendamentoPorIdAsync(int id)
        {
            return await _agendamentoRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Servico>> ObterServicosAsync()
        {
            return await _servicoRepository.GetAllAsync();
        }

        public async Task<int> CriarAgendamentoAsync(int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds, string formaPagamento, decimal precoTotal, string paymentId = null)
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
                PaymentId = paymentId,
                Status = StatusAgendamento.Pendente,
                StatusPagamento = StatusPagamento.Pendente,
                AgendamentoServicos = servicos.Select(s => new AgendamentoServico { ServicoId = s.ServicoId }).ToList()
            };

            var agendamento = await _agendamentoRepository.AddAsync(novoAgendamento);
            return agendamento.AgendamentoId;
        }

        public async Task<bool> UpdateAgendamentoStatusByPaymentIdAsync(string paymentId, StatusPagamento novoStatus)
        {
            var agendamento = await _agendamentoRepository.ObterPorPaymentIdAsync(paymentId);
            if (agendamento == null) return false;

            agendamento.StatusPagamento = novoStatus;
            await _agendamentoRepository.UpdateAsync(agendamento);
            return true;
        }
    }
}
