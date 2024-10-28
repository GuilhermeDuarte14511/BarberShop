using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        // Implementação do método GetAvailableSlotsAsync
        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime date, int duracaoTotal)
        {
            List<DateTime> horariosDisponiveis = new List<DateTime>();

            // Encontrar a primeira segunda-feira da semana da data fornecida
            DateTime startOfWeek = date.Date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);

            // Iterar de segunda a sexta
            for (int i = 0; i < 5; i++)
            {
                DateTime diaAtual = startOfWeek.AddDays(i);

                // Obtém todos os agendamentos para o barbeiro na data específica
                var agendamentos = await _context.Agendamentos
                    .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == diaAtual.Date)
                    .ToListAsync();

                DateTime horarioInicial = diaAtual.Date.AddHours(9);  // Horário de início do expediente (09:00)
                DateTime horarioFimDia = diaAtual.Date.AddHours(18);   // Horário de fim do expediente (18:00)

                while (horarioInicial.AddMinutes(duracaoTotal) <= horarioFimDia)
                {
                    // Verifica se o horário está ocupado por outro agendamento
                    bool horarioOcupado = agendamentos.Any(a =>
                        (horarioInicial >= a.DataHora && horarioInicial < a.DataHora.AddMinutes(a.DuracaoTotal)) ||
                        (horarioInicial.AddMinutes(duracaoTotal) > a.DataHora && horarioInicial.AddMinutes(duracaoTotal) <= a.DataHora.AddMinutes(a.DuracaoTotal)));

                    // Se o horário não estiver ocupado, adiciona à lista de horários disponíveis
                    if (!horarioOcupado)
                    {
                        horariosDisponiveis.Add(horarioInicial);
                    }

                    // Avança para o próximo horário possível
                    horarioInicial = horarioInicial.AddMinutes(duracaoTotal);
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
    }
}
