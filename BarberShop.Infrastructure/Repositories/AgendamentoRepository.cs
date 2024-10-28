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
        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime dataVisualizacao, int duracaoTotal)
        {
            var horariosDisponiveis = new List<DateTime>();

            // Data de início é a data de visualização sem o componente de tempo
            DateTime dataInicio = dataVisualizacao.Date;

            // Encontrar a data do próximo domingo a partir da data de início
            int diasAteDomingo = ((int)DayOfWeek.Sunday - (int)dataInicio.DayOfWeek + 7) % 7;
            DateTime dataFim = dataInicio.AddDays(diasAteDomingo);

            // Iterar de dataInicio até dataFim, um dia de cada vez
            for (DateTime dataAtual = dataInicio; dataAtual <= dataFim; dataAtual = dataAtual.AddDays(1))
            {
                // Pular as segundas-feiras (sem expediente)
                if (dataAtual.DayOfWeek == DayOfWeek.Monday)
                    continue;

                // Obter todos os agendamentos do barbeiro para o dia atual
                var agendamentosDoDia = await _context.Agendamentos
                    .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == dataAtual.Date)
                    .ToListAsync();

                // Definir horário de abertura e fechamento do expediente (09:00 às 18:00)
                DateTime horarioAbertura = dataAtual.AddHours(9);
                DateTime horarioFechamento = dataAtual.AddHours(18);

                // Iniciar o horário atual como o horário de abertura
                DateTime horarioAtual = horarioAbertura;

                // Iterar pelos horários disponíveis no dia atual
                while (horarioAtual.AddMinutes(duracaoTotal) <= horarioFechamento)
                {
                    // Verificar se o horário atual conflita com algum agendamento existente
                    bool existeConflito = agendamentosDoDia.Any(agendamento =>
                        horarioAtual < agendamento.DataHora.AddMinutes(agendamento.DuracaoTotal) &&
                        horarioAtual.AddMinutes(duracaoTotal) > agendamento.DataHora
                    );

                    // Se não houver conflito, adicionar o horário à lista de horários disponíveis
                    if (!existeConflito)
                    {
                        horariosDisponiveis.Add(horarioAtual);
                    }

                    // Avançar para o próximo intervalo de tempo
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
    }
}
