using BarberShop.Application.DTOs;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

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
        private readonly IPagamentoService _pagamentoService;

        public AgendamentoService(
            IAgendamentoRepository agendamentoRepository,
            IServicoRepository servicoRepository,
            IPagamentoRepository pagamentoRepository,
            IBarbeariaRepository barbeariaRepository,
            IFeriadoNacionalRepository feriadoNacionalRepository,
            IFeriadoBarbeariaRepository feriadoBarbeariaRepository,
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IPagamentoService pagamentoService
        )
        {
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository;
            _pagamentoRepository = pagamentoRepository;
            _barbeariaRepository = barbeariaRepository;
            _feriadoNacionalRepository = feriadoNacionalRepository;
            _feriadoBarbeariaRepository = feriadoBarbeariaRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _pagamentoService = pagamentoService;
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

        public async Task<(IEnumerable<DateTime> HorariosDisponiveis, Dictionary<DayOfWeek, (TimeSpan abertura, TimeSpan fechamento)> HorarioFuncionamento)> ObterHorariosDisponiveisAsync(int barbeariaId, int barbeiroId, DateTime data, int duracaoTotal)
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

            // Obter indisponibilidades do barbeiro
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

            // Retorna os horários disponíveis e o horário de funcionamento
            return (horariosDisponiveis, horarioFuncionamento);
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

        public async Task<List<Agendamento>> ObterAgendamentosConcluidosAsync()
        {
            var agendamentos = await _agendamentoRepository.ObterAgendamentosConcluidosSemEmailAsync();
            return agendamentos.ToList();
        }

        public async Task AtualizarAgendamentoAsync(int id, Agendamento agendamentoAtualizado)
        {
            // Buscar o agendamento no banco de dados
            var agendamentoExistente = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamentoExistente == null)
            {
                throw new Exception("Agendamento não encontrado.");
            }

            // Atualizar os campos permitidos no agendamento
            agendamentoExistente.DataHora = agendamentoAtualizado.DataHora;
            agendamentoExistente.Status = agendamentoAtualizado.Status;
            agendamentoExistente.PrecoTotal = agendamentoAtualizado.PrecoTotal;

            // Verificar se há um pagamento vinculado
            if (agendamentoExistente.Pagamento != null)
            {
                // Atualizar os campos do pagamento existente
                agendamentoExistente.Pagamento.StatusPagamento = agendamentoAtualizado.Pagamento?.StatusPagamento ?? agendamentoExistente.Pagamento.StatusPagamento;
                agendamentoExistente.Pagamento.ValorPago = agendamentoAtualizado.PrecoTotal ?? agendamentoExistente.Pagamento.ValorPago;

                // Atualizar a data de pagamento, se necessário
                if (agendamentoAtualizado.Pagamento?.StatusPagamento == StatusPagamento.Aprovado)
                {
                    agendamentoExistente.Pagamento.DataPagamento = DateTime.UtcNow;
                }

                // Persistir alterações no pagamento
                await _pagamentoService.AtualizarPagamentoAsync(agendamentoExistente.Pagamento);
            }
            else if (agendamentoAtualizado.Pagamento != null)
            {
                // Criar um novo pagamento se ainda não existir e foi enviado um no agendamento atualizado
                var novoPagamento = new Pagamento
                {
                    AgendamentoId = id,
                    ClienteId = agendamentoExistente.ClienteId,
                    BarbeariaId = agendamentoExistente.BarbeariaId,
                    ValorPago = agendamentoAtualizado.PrecoTotal ?? 0,
                    StatusPagamento = agendamentoAtualizado.Pagamento.StatusPagamento,
                    DataPagamento = agendamentoAtualizado.Pagamento.StatusPagamento == StatusPagamento.Aprovado
                        ? DateTime.UtcNow
                        : null
                };

                // Adicionar o novo pagamento
                await _pagamentoService.AdicionarPagamentoAsync(novoPagamento);
                agendamentoExistente.Pagamento = novoPagamento;
            }
            else
            {
                throw new Exception("Nenhum pagamento vinculado encontrado e nenhuma atualização de pagamento fornecida.");
            }

            // Persistir alterações no agendamento
            await _agendamentoRepository.UpdateAsync(agendamentoExistente);
        }

        public async Task<List<Agendamento>> ObterAgendamentosFuturosPorBarbeiroIdAsync(int barbeiroId)
        {
            var hoje = DateTime.Today;
            var agendamentosFuturos = await _agendamentoRepository.ObterAgendamentosPorBarbeiroIdAsync(barbeiroId, hoje);
            return agendamentosFuturos.ToList();
        }

        public async Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEBarbeariaAsync(int barbeiroId, int barbeariaId, int? agendamentoId = null)
        {
            return await _agendamentoRepository.ObterAgendamentosPorBarbeiroEBarbeariaAsync(barbeiroId, barbeariaId, agendamentoId);
        }

        public async Task<IEnumerable<Agendamento>> FiltrarAgendamentosAsync(int? barbeiroId, int barbeariaId, string clienteNome = null, DateTime? dataInicio = null, DateTime? dataFim = null, string formaPagamento = null,
                                                                             StatusAgendamento? status = null, StatusPagamento? statusPagamento = null, string barbeiroNome = null, int? agendamentoId = null) // Adicionado agendamentoId
        {
            return await _agendamentoRepository.FiltrarAgendamentosAsync(
                barbeiroId,
                barbeariaId,
                clienteNome,
                dataInicio,
                dataFim,
                formaPagamento,
                status,
                statusPagamento,
                barbeiroNome,
                agendamentoId);
        }

        public async Task AtualizarAgendamentoAsync(int id, AgendamentoDto agendamentoAtualizadoDto)
        {
            var agendamentoExistente = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamentoExistente == null)
            {
                throw new Exception("Agendamento não encontrado.");
            }

            // Atualiza os campos básicos do agendamento
            agendamentoExistente.DataHora = agendamentoAtualizadoDto.DataHora.Value;
            agendamentoExistente.Status = agendamentoAtualizadoDto.Status;
            agendamentoExistente.PrecoTotal = agendamentoAtualizadoDto.PrecoTotal ?? agendamentoExistente.PrecoTotal.GetValueOrDefault();

            // Verifica se o pagamento já existe
            if (agendamentoExistente.Pagamento != null)
            {
                if (Enum.TryParse(agendamentoAtualizadoDto.StatusPagamento, out StatusPagamento statusPagamento))
                {
                    agendamentoExistente.Pagamento.StatusPagamento = statusPagamento;
                }
                agendamentoExistente.Pagamento.ValorPago = agendamentoAtualizadoDto.PrecoTotal ?? agendamentoExistente.Pagamento.ValorPago.GetValueOrDefault();

                // Atualiza a data de pagamento, se necessário
                if (agendamentoExistente.Pagamento.StatusPagamento == StatusPagamento.Aprovado)
                {
                    agendamentoExistente.Pagamento.DataPagamento = DateTime.UtcNow;
                }

                await _pagamentoService.AtualizarPagamentoAsync(agendamentoExistente.Pagamento);
            }
            else if (!string.IsNullOrEmpty(agendamentoAtualizadoDto.StatusPagamento))
            {
                // Cria um novo pagamento se não existir e há informações no DTO
                if (Enum.TryParse(agendamentoAtualizadoDto.StatusPagamento, out StatusPagamento novoStatusPagamento))
                {
                    var novoPagamento = new Pagamento
                    {
                        AgendamentoId = id,
                        ClienteId = agendamentoExistente.ClienteId,
                        BarbeariaId = agendamentoExistente.BarbeariaId,
                        ValorPago = agendamentoAtualizadoDto.PrecoTotal ?? 0, // Se for nulo, usa 0 como padrão
                        StatusPagamento = novoStatusPagamento,
                        DataPagamento = novoStatusPagamento == StatusPagamento.Aprovado
                            ? DateTime.UtcNow
                            : null
                    };

                    await _pagamentoService.AdicionarPagamentoAsync(novoPagamento);
                    agendamentoExistente.Pagamento = novoPagamento;
                }
            }

            // Atualiza o agendamento no repositório
            await _agendamentoRepository.UpdateAsync(agendamentoExistente);
        }

        public async Task<AgendamentoDto> ObterAgendamentoCompletoPorIdAsync(int id)
        {
            var agendamento = await _agendamentoRepository.ObterAgendamentoCompletoPorIdAsync(id);

            if (agendamento == null)
                return null;

            return new AgendamentoDto
            {
                AgendamentoId = agendamento.AgendamentoId,
                DataHora = agendamento.DataHora,
                Status = agendamento.Status,
                PrecoTotal = agendamento.PrecoTotal,
                StatusPagamento = agendamento.Pagamento?.StatusPagamento.ToString() ?? "Não Especificado",
                FormaPagamento = agendamento.FormaPagamento,
                Cliente = new ClienteDTO
                {
                    Nome = agendamento.Cliente?.Nome
                },
                BarbeiroNome = agendamento.Barbeiro?.Nome,
                Servicos = agendamento.AgendamentoServicos
                    .Select(agendamentoServico => new ServicoDTO
                    {
                        ServicoId = agendamentoServico.Servico.ServicoId,
                        Nome = agendamentoServico.Servico.Nome,
                        Preco = (decimal)agendamentoServico.Servico.Preco
                    }).ToList(),
                Pagamento = agendamento.Pagamento
            };
        }


    }
}
