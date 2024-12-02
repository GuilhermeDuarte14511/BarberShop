using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class AvaliacaoService : IAvaliacaoService
    {
        private readonly IAvaliacaoRepository _avaliacaoRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly INotificacaoService _notificacaoService;

        public AvaliacaoService(IAvaliacaoRepository avaliacaoRepository, IAgendamentoRepository agendamentoRepository, INotificacaoService notificacaoService)
        {
            _avaliacaoRepository = avaliacaoRepository;
            _agendamentoRepository = agendamentoRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task<Avaliacao> AdicionarAvaliacaoAsync(Avaliacao avaliacao)
        {
            // Valida se o agendamento existe
            var agendamento = await _agendamentoRepository.GetByIdAsync(avaliacao.AgendamentoId);
            if (agendamento == null)
            {
                throw new ArgumentException("Agendamento não encontrado.");
            }

            // Valida se já existe avaliação para o agendamento
            var avaliacaoExistente = await _avaliacaoRepository.ObterAvaliacaoPorAgendamentoIdAsync(avaliacao.AgendamentoId);
            if (avaliacaoExistente != null)
            {
                throw new InvalidOperationException("Já existe uma avaliação para este agendamento.");
            }

            // Adiciona a avaliação ao banco
            var novaAvaliacao = await _avaliacaoRepository.AdicionarAvaliacaoAsync(avaliacao);

            // Notificar admins e barbeiro sobre a nova avaliação
            _notificacaoService.NotificarAvaliacaoRecebida(novaAvaliacao);

            return novaAvaliacao;
        }


        public async Task<Agendamento> ObterAgendamentoPorIdAsync(int agendamentoId)
        {
            // Busca o agendamento pelo ID
            return await _agendamentoRepository.GetDataAvaliacaoAsync(agendamentoId);
        }

        public async Task<Avaliacao> ObterAvaliacaoPorAgendamentoIdAsync(int agendamentoId)
        {
            // Busca a avaliação pelo ID do agendamento
            return await _avaliacaoRepository.ObterAvaliacaoPorAgendamentoIdAsync(agendamentoId);
        }

        public async Task<IEnumerable<Avaliacao>> ObterAvaliacoesFiltradasAsync(int? barbeariaId = null,int? barbeiroId = null,string? dataInicio = null,string? dataFim = null,int? notaServico = null,int? notaBarbeiro = null,string? observacao = null)
        {
            return await _avaliacaoRepository.ObterAvaliacoesFiltradasAsync(
                barbeariaId,
                barbeiroId,
                dataInicio,
                dataFim,
                notaServico,
                notaBarbeiro,
                observacao);
        }

        public async Task<IEnumerable<Avaliacao>> ObterAvaliacoesPorBarbeiroIdAsync(int barbeiroId, int? avaliacaoId = null)
        {
            return await _avaliacaoRepository.ObterAvaliacoesPorBarbeiroIdAsync(barbeiroId, avaliacaoId);
        }


    }
}
