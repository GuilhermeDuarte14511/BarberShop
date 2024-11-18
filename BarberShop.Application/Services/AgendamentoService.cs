﻿using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;

namespace BarberShop.Application.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly IBarbeariaRepository _barbeariaRepository;
        private readonly IFeriadoNacionalRepository _feriadoNacionalRepository;
        private readonly IFeriadoBarbeariaRepository _feriadoBarbeariaRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;

        public AgendamentoService(
            IAgendamentoRepository agendamentoRepository,
            IServicoRepository servicoRepository,
            IPagamentoRepository pagamentoRepository,
            IBarbeariaRepository barbeariaRepository,
            IFeriadoNacionalRepository feriadoNacionalRepository,
            IFeriadoBarbeariaRepository feriadoBarbeariaRepository,
            IIndisponibilidadeRepository indisponibilidadeRepository
        )
        {
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository;
            _pagamentoRepository = pagamentoRepository;
            _barbeariaRepository = barbeariaRepository;
            _feriadoNacionalRepository = feriadoNacionalRepository;
            _feriadoBarbeariaRepository = feriadoBarbeariaRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
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

        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeariaId, int barbeiroId, DateTime data, int duracaoTotal)
        {
            var barbearia = await _barbeariaRepository.GetByIdAsync(barbeariaId);
            if (barbearia == null || string.IsNullOrEmpty(barbearia.HorarioFuncionamento))
            {
                throw new Exception("Horário de funcionamento não encontrado para a barbearia.");
            }

            var horarioFuncionamento = ParseHorarioFuncionamento(barbearia.HorarioFuncionamento);

            // Obter feriados nacionais e da barbearia
            var feriadosNacionais = await _feriadoNacionalRepository.ObterTodosFeriadosAsync();
            var feriadosBarbearia = await _feriadoBarbeariaRepository.ObterFeriadosPorBarbeariaAsync(barbeariaId);

            // Combinar todos os feriados
            var datasFeriadosNacionais = feriadosNacionais.Select(f => f.Data);
            var datasFeriadosBarbearia = feriadosBarbearia.Select(f => f.Data);
            var feriados = datasFeriadosNacionais
                .Concat(datasFeriadosBarbearia)
                .ToHashSet();

            // Obter indisponibilidades do barbeiro e converter para o formato esperado
            var indisponibilidades = (await _indisponibilidadeRepository.ObterIndisponibilidadesPorBarbeiroAsync(barbeiroId))
                .Select(i => (i.DataInicio, i.DataFim))
                .ToList();

            // Passar todas as restrições para o repositório
            var horariosDisponiveis = await _agendamentoRepository.GetAvailableSlotsAsync(
                barbeariaId,
                barbeiroId,
                data,
                duracaoTotal,
                horarioFuncionamento,
                feriados,
                indisponibilidades
            );

            return horariosDisponiveis;
        }





        private Dictionary<DayOfWeek, (TimeSpan abertura, TimeSpan fechamento)> ParseHorarioFuncionamento(string horarioFuncionamento)
        {
            var horarioPorDia = new Dictionary<DayOfWeek, (TimeSpan abertura, TimeSpan fechamento)>();
            var diasHorarios = horarioFuncionamento.Split(',');

            // Mapeamento dos dias em português para DayOfWeek em inglês
            var diasSemanaMap = new Dictionary<string, DayOfWeek>
            {
                { "Seg", DayOfWeek.Monday },
                { "Ter", DayOfWeek.Tuesday },
                { "Qua", DayOfWeek.Wednesday },
                { "Qui", DayOfWeek.Thursday },
                { "Sex", DayOfWeek.Friday },
                { "Sab", DayOfWeek.Saturday },
                { "Dom", DayOfWeek.Sunday }
            };

            foreach (var diaHorario in diasHorarios)
            {
                var partes = diaHorario.Trim().Split(' ');
                var dias = partes[0].Split('-');
                var horas = partes[1].Split('-');
                var abertura = TimeSpan.Parse(horas[0]);
                var fechamento = TimeSpan.Parse(horas[1]);

                if (diasSemanaMap.TryGetValue(dias[0], out DayOfWeek diaInicio) &&
                    diasSemanaMap.TryGetValue(dias[^1], out DayOfWeek diaFim))
                {
                    for (var dia = diaInicio; dia <= diaFim; dia++)
                    {
                        horarioPorDia[dia] = (abertura, fechamento);
                    }
                }
            }

            return horarioPorDia;
        }

        public async Task<int> CriarAgendamentoAsync(int barbeariaId, int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds, string formaPagamento, decimal precoTotal)
        {
            try
            {
                var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIds);
                var duracaoTotal = servicos.Sum(s => s.Duracao);

                var novoAgendamento = new Agendamento
                {
                    BarbeariaId = barbeariaId,
                    BarbeiroId = barbeiroId,
                    ClienteId = clienteId,
                    DataHora = dataHora,
                    DuracaoTotal = duracaoTotal,
                    FormaPagamento = formaPagamento,
                    PrecoTotal = precoTotal,
                    AgendamentoServicos = servicos.Select(s => new AgendamentoServico { ServicoId = s.ServicoId }).ToList()
                };

                // Adiciona o agendamento no contexto, mas não salva ainda
                var agendamento = await _agendamentoRepository.AddAsync(novoAgendamento);

                // Salva as mudanças no contexto para garantir que o ID do agendamento seja gerado
                await _agendamentoRepository.SaveChangesAsync();

                var pagamento = new Pagamento
                {
                    AgendamentoId = agendamento.AgendamentoId, // Agora temos certeza de que o ID está disponível
                    ClienteId = clienteId,
                    BarbeariaId = barbeariaId,
                    ValorPago = precoTotal,
                    StatusPagamento = StatusPagamento.Pendente
                };

                // Adiciona o pagamento no contexto, mas não salva ainda
                await _pagamentoRepository.AddAsync(pagamento);

                // Salva as mudanças no contexto para persistir o pagamento
                await _pagamentoRepository.SaveChangesAsync();

                return agendamento.AgendamentoId;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao criar o agendamento.", ex);
            }
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
