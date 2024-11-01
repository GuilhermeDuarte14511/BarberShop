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
            return entity;  // Retorna o agendamento adicionado
        }

        // Implementação do método UpdateAsync
        public async Task UpdateAsync(Agendamento entity)
        {
            _context.Agendamentos.Update(entity);  // Atualiza o agendamento existente
            await _context.SaveChangesAsync();  // Salva as alterações no banco de dados
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
                                     .ThenInclude(ags => ags.Servico)  // Inclui os serviços relacionados
                                 .ToListAsync();
        }

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime dataVisualizacao, int duracaoTotal)
        {
            var horariosDisponiveis = new List<DateTime>();

            DateTime dataInicio = dataVisualizacao.Date;
            int diasAteDomingo = ((int)DayOfWeek.Sunday - (int)dataInicio.DayOfWeek + 7) % 7;
            DateTime dataFim = dataInicio.AddDays(diasAteDomingo);

            for (DateTime dataAtual = dataInicio; dataAtual <= dataFim; dataAtual = dataAtual.AddDays(1))
            {
                // Ignora as segundas-feiras (ou qualquer dia que o barbeiro não trabalhe)
                if (dataAtual.DayOfWeek == DayOfWeek.Monday)
                    continue;

                // Obtem agendamentos do dia para o barbeiro
                var agendamentosDoDia = await _context.Agendamentos
                    .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == dataAtual.Date)
                    .ToListAsync();

                DateTime horarioAbertura = dataAtual.AddHours(9);
                DateTime horarioFechamento = dataAtual.AddHours(18);

                DateTime horarioAtual = horarioAbertura;

                // Loop para verificar disponibilidade de horários com a duração total do serviço
                while (horarioAtual.AddMinutes(duracaoTotal) <= horarioFechamento)
                {
                    DateTime horarioFimProposto = horarioAtual.AddMinutes(duracaoTotal);

                    // Verificar se o horário proposto conflita com qualquer agendamento existente
                    bool existeConflito = agendamentosDoDia.Any(agendamento =>
                        horarioAtual < agendamento.DataHora.AddMinutes(agendamento.DuracaoTotal ?? 0) &&
                        horarioFimProposto > agendamento.DataHora
                    );

                    // Se não houver conflito, adiciona o horário como disponível
                    if (!existeConflito)
                    {
                        horariosDisponiveis.Add(horarioAtual);
                    }

                    // Incrementa o horário atual para o próximo intervalo
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

    }
}
