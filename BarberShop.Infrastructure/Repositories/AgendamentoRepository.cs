using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class AgendamentoRepository : IAgendamentoRepository
    {
        private readonly BarbeariaContext _context;

        public AgendamentoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        // Implementação do método AddAsync
        public async Task<Agendamento> AddAsync(Agendamento entity)
        {
            await _context.Agendamentos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Implementação do método UpdateAsync
        public async Task UpdateAsync(Agendamento entity)
        {
            _context.Agendamentos.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Implementação do método DeleteAsync
        public async Task DeleteAsync(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento != null)
            {
                _context.Agendamentos.Remove(agendamento);
                await _context.SaveChangesAsync();
            }
        }

        // Implementação do método GetAllAsync
        public async Task<IEnumerable<Agendamento>> GetAllAsync()
        {
            return await _context.Agendamentos.Include(a => a.Cliente)
                                               .Include(a => a.Barbeiro)
                                               .ToListAsync();
        }

        // Implementação do método GetByIdAsync
        public async Task<Agendamento> GetByIdAsync(int id)
        {
            return await _context.Agendamentos.Include(a => a.Cliente)
                                              .Include(a => a.Barbeiro)
                                              .FirstOrDefaultAsync(a => a.AgendamentoId == id);
        }

        // Implementação do método GetByClienteIdAsync
        public async Task<IEnumerable<Agendamento>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Agendamentos.Where(a => a.ClienteId == clienteId)
                                              .Include(a => a.Cliente)
                                              .Include(a => a.Barbeiro)
                                              .ToListAsync();
        }

        public async Task<Agendamento> GetDataAvaliacaoAsync(int agendamentoId)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Include(a => a.AgendamentoServicos)
                    .ThenInclude(asg => asg.Servico) // Inclui os serviços
                .Include(a => a.Avaliacoes) // Inclui as avaliações
                .FirstOrDefaultAsync(a => a.AgendamentoId == agendamentoId);
        }

        // Implementação de GetAgendamentosPorBarbeariaAsync
        public async Task<IEnumerable<Agendamento>> GetAgendamentosPorBarbeariaAsync(int barbeariaId)
        {
            return await _context.Agendamentos
                .Where(a => a.BarbeariaId == barbeariaId)
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Include(a => a.AgendamentoServicos)
                    .ThenInclude(asg => asg.Servico)
                .ToListAsync();
        }

        public async Task<Agendamento> GetByIdAndBarbeariaIdAsync(int id, int barbeariaId)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Include(a => a.AgendamentoServicos)
                    .ThenInclude(asg => asg.Servico)
                .FirstOrDefaultAsync(a => a.AgendamentoId == id && a.BarbeariaId == barbeariaId);
        }


        // Implementação de GetByClienteIdWithServicosAsync com clienteId e barbeariaId
        public async Task<IEnumerable<Agendamento>> GetByClienteIdWithServicosAsync(int clienteId, int? barbeariaId)
        {
            return await _context.Agendamentos
                .Where(a => a.ClienteId == clienteId && a.BarbeariaId == barbeariaId)
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Include(a => a.Pagamento)
                .Include(a => a.AgendamentoServicos)
                    .ThenInclude(ags => ags.Servico)
                .ToListAsync();
        }

        // Método para verificar a disponibilidade de horário específico
        public async Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao)
        {
            // Registrar log de depuração com duração total
            await LogAgendamentoDebugAsync(nameof(VerificarDisponibilidadeHorarioAsync), "Iniciando verificação de horário", dataHora, barbeiroId, duracao);

            DateTime horarioInicio = dataHora;
            DateTime horarioFim = dataHora.AddMinutes(duracao);

            var agendamentosConflitantes = await _context.Agendamentos
                .Where(a => a.BarbeiroId == barbeiroId &&
                            ((a.DataHora <= horarioInicio && a.DataHora.AddMinutes(a.DuracaoTotal ?? 0) > horarioInicio) ||
                             (a.DataHora < horarioFim && a.DataHora.AddMinutes(a.DuracaoTotal ?? 0) >= horarioFim)))
                .ToListAsync();

            bool disponibilidade = !agendamentosConflitantes.Any();

            await LogAgendamentoDebugAsync(nameof(VerificarDisponibilidadeHorarioAsync), $"Verificação concluída. Disponível: {disponibilidade}", dataHora, barbeiroId, duracao);

            return disponibilidade;
        }

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeariaId, int barbeiroId, DateTime dataVisualizacao, int duracaoTotal, Dictionary<DayOfWeek, (TimeSpan abertura, TimeSpan fechamento)> horarioFuncionamento)
        {
            var horariosDisponiveis = new List<DateTime>();

            // Define o fuso horário de Brasília
            TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

            // Define o início e o fim do horário de exibição baseado no horário da barbearia
            DateTime dataInicio = dataVisualizacao.Date.AddHours(9);
            dataInicio = TimeZoneInfo.ConvertTime(dataInicio, brasiliaTimeZone);
            dataInicio = dataInicio.AddHours(3);

            if (DateTime.Now > dataInicio)
                dataInicio = DateTime.Now;

            DateTime dataFim = dataInicio.AddDays(7).Date.AddHours(18);

            for (DateTime dataAtual = dataInicio; dataAtual <= dataFim; dataAtual = dataAtual.AddDays(1).Date.AddHours(9))
            {
                if (!horarioFuncionamento.TryGetValue(dataAtual.DayOfWeek, out var funcionamentoDia))
                    continue;

                DateTime horarioAbertura = dataAtual.Date.Add(funcionamentoDia.abertura);
                DateTime horarioFechamento = dataAtual.Date.Add(funcionamentoDia.fechamento);
                DateTime horarioAtual = horarioAbertura;

                var agendamentosDoDia = await _context.Agendamentos
                    .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == dataAtual.Date)
                    .OrderBy(a => a.DataHora)
                    .ToListAsync();

                foreach (var agendamento in agendamentosDoDia)
                {
                    DateTime inicioAgendamento = agendamento.DataHora;
                    DateTime fimAgendamento = inicioAgendamento.AddMinutes(agendamento.DuracaoTotal ?? 0);

                    while (horarioAtual.AddMinutes(duracaoTotal) <= inicioAgendamento)
                    {
                        if (horarioAtual >= DateTime.Now)
                        {
                            horariosDisponiveis.Add(horarioAtual);
                        }
                        horarioAtual = horarioAtual.AddMinutes(duracaoTotal);
                    }

                    horarioAtual = fimAgendamento;
                    if (horarioAtual >= horarioFechamento)
                        break;
                }

                while (horarioAtual.AddMinutes(duracaoTotal) <= horarioFechamento)
                {
                    if (horarioAtual >= DateTime.Now)
                    {
                        horariosDisponiveis.Add(horarioAtual);
                    }
                    horarioAtual = horarioAtual.AddMinutes(duracaoTotal);
                }
            }

            return horariosDisponiveis;
        }



        // Implementação do método ObterAgendamentosPorBarbeiroIdAsync
        public async Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroIdAsync(int barbeiroId, DateTime data)
        {
            return await _context.Agendamentos
                .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == data.Date)
                .ToListAsync();
        }

        // Implementação do método ObterAgendamentosPorBarbeiroEHorarioAsync
        public async Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEHorarioAsync(int barbeiroId, DateTime dataHoraInicio, DateTime dataHoraFim)
        {
            return await _context.Agendamentos
                .Where(a => a.BarbeiroId == barbeiroId &&
                            a.DataHora < dataHoraFim &&
                            a.DataHora.AddMinutes(a.DuracaoTotal ?? 0) > dataHoraInicio)
                .ToListAsync();
        }

        public async Task AtualizarStatusPagamentoAsync(int agendamentoId, StatusPagamento statusPagamento, string paymentId = null)
        {
            var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.AgendamentoId == agendamentoId);
            if (pagamento != null)
            {
                pagamento.StatusPagamento = statusPagamento;
                pagamento.PaymentId = paymentId;

                _context.Entry(pagamento).Property(p => p.StatusPagamento).IsModified = true;
                _context.Entry(pagamento).Property(p => p.PaymentId).IsModified = true;

                await _context.SaveChangesAsync();
            }
            else
            {
                // Caso não exista um pagamento para o agendamento, pode-se decidir criar um novo ou lançar uma exceção.
                var novoPagamento = new Pagamento
                {
                    AgendamentoId = agendamentoId,
                    StatusPagamento = statusPagamento,
                    PaymentId = paymentId,
                    DataPagamento = DateTime.UtcNow // ou outra lógica para a data de pagamento
                };

                await _context.Pagamentos.AddAsync(novoPagamento);
                await _context.SaveChangesAsync();
            }
        }


        // Método de log para depuração de agendamento, incluindo a duração total
        private async Task LogAgendamentoDebugAsync(string source, string message, DateTime dataHora, int barbeiroId, int? duracao = null)
        {
            // Define a duração total como "00" se o valor não estiver presente
            string duracaoTotal = duracao.HasValue ? duracao.Value.ToString() : "00";

            var log = new Log
            {
                LogDateTime = DateTime.UtcNow,
                LogLevel = "DEBUG",
                Source = source,
                Message = message,
                Data = $"DataHora: {dataHora}, BarbeiroId: {barbeiroId}, DuracaoTotal: {duracaoTotal}",
                ResourceID = null
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetAgendamentosPorPeriodoAsync(int barbeariaId, DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Agendamentos
                .Where(agendamento => agendamento.BarbeariaId == barbeariaId &&
                                      agendamento.DataHora.Date >= dataInicio.Date &&
                                      agendamento.DataHora.Date <= dataFim.Date)
                .Include(agendamento => agendamento.Cliente)
                .Include(agendamento => agendamento.Barbeiro) // Inclui o barbeiro
                .Include(agendamento => agendamento.AgendamentoServicos) // Inclui os serviços do agendamento
                    .ThenInclude(agendamentoServico => agendamentoServico.Servico) // Inclui detalhes de cada serviço
                .ToListAsync();
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }



    }
}
