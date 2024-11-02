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

        // Implementação do método GetByClienteIdWithServicosAsync
        public async Task<IEnumerable<Agendamento>> GetByClienteIdWithServicosAsync(int clienteId)
        {
            return await _context.Agendamentos
                                 .Where(a => a.ClienteId == clienteId)
                                 .Include(a => a.Cliente)
                                 .Include(a => a.Barbeiro)
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

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime dataVisualizacao, int duracaoTotal)
        {
            var horariosDisponiveis = new List<DateTime>();

            DateTime dataInicio = dataVisualizacao.Date;
            int diasAteDomingo = ((int)DayOfWeek.Sunday - (int)dataInicio.DayOfWeek + 7) % 7;
            DateTime dataFim = dataInicio.AddDays(diasAteDomingo);

            for (DateTime dataAtual = dataInicio; dataAtual <= dataFim; dataAtual = dataAtual.AddDays(1))
            {
                if (dataAtual.DayOfWeek == DayOfWeek.Monday)
                    continue;

                // Obter todos os agendamentos do barbeiro para o dia específico, ordenados pelo horário de início
                var agendamentosDoDia = await _context.Agendamentos
                    .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == dataAtual.Date)
                    .OrderBy(a => a.DataHora)
                    .ToListAsync();

                DateTime horarioAbertura = dataAtual.AddHours(9);  // Início do expediente às 9h
                DateTime horarioFechamento = dataAtual.AddHours(18);  // Fim do expediente às 18h
                DateTime horarioAtual = horarioAbertura;

                foreach (var agendamento in agendamentosDoDia)
                {
                    DateTime inicioAgendamento = agendamento.DataHora;
                    DateTime fimAgendamento = inicioAgendamento.AddMinutes(agendamento.DuracaoTotal ?? 0);

                    // Adicionar todos os horários disponíveis até o início do próximo agendamento
                    while (horarioAtual.AddMinutes(duracaoTotal) <= inicioAgendamento)
                    {
                        horariosDisponiveis.Add(horarioAtual);
                        horarioAtual = horarioAtual.AddMinutes(duracaoTotal);
                    }

                    // Avançar o horário atual para o fim do agendamento atual
                    horarioAtual = fimAgendamento;

                    // Se o horário atual ultrapassar o horário de fechamento, interrompe o loop
                    if (horarioAtual >= horarioFechamento)
                        break;
                }

                // Adicionar horários após o último agendamento do dia até o horário de fechamento
                while (horarioAtual.AddMinutes(duracaoTotal) <= horarioFechamento)
                {
                    horariosDisponiveis.Add(horarioAtual);
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
            var agendamento = await _context.Agendamentos.FindAsync(agendamentoId);
            if (agendamento != null)
            {
                agendamento.StatusPagamento = statusPagamento;
                agendamento.PaymentId = paymentId;

                _context.Entry(agendamento).Property(a => a.StatusPagamento).IsModified = true;
                _context.Entry(agendamento).Property(a => a.PaymentId).IsModified = true;

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

    }
}
