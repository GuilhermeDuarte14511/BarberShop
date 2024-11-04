using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly BarbeariaContext _context;

        public DashboardRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<int[]> GetAgendamentosPorSemanaAsync()
        {
            var startDate = DateTime.Now.AddDays(-7).Date;

            // Traz os dados para a memória e então agrupa por dia da semana
            var agendamentosDaSemana = _context.Agendamentos
                .Where(a => a.DataHora >= startDate && a.DataHora <= DateTime.Now)
                .AsEnumerable() // Executa a consulta na memória
                .GroupBy(a => a.DataHora.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToArray(); // `ToArray()` sem await

            return await Task.FromResult(agendamentosDaSemana);
        }

        public async Task<Dictionary<string, int>> GetServicosMaisSolicitadosAsync()
        {
            // Carregar os dados na memória antes de agrupar por serviço
            var agendamentoServicos = await _context.AgendamentoServicos
                .Include(ag => ag.Servico)
                .ToListAsync();

            var servicosMaisSolicitados = agendamentoServicos
                .GroupBy(ag => ag.Servico.Nome)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .ToDictionary(g => g.Key, g => g.Count());

            return servicosMaisSolicitados;
        }

        public async Task<Dictionary<string, decimal>> GetLucroPorBarbeiroAsync()
        {
            var lucroPorBarbeiro = await _context.Agendamentos
                .Where(a => a.Status == StatusAgendamento.Concluido && a.PrecoTotal.HasValue)
                .GroupBy(a => a.Barbeiro.Nome)
                .Select(g => new { Barbeiro = g.Key, Lucro = g.Sum(a => a.PrecoTotal.Value) })
                .ToDictionaryAsync(x => x.Barbeiro, x => x.Lucro);

            return lucroPorBarbeiro;
        }

        public async Task<Dictionary<string, int>> GetAtendimentosPorBarbeiroAsync()
        {
            var atendimentosPorBarbeiro = await _context.Agendamentos
                .GroupBy(a => a.Barbeiro.Nome)
                .Select(g => new { Barbeiro = g.Key, Atendimentos = g.Count() })
                .ToDictionaryAsync(x => x.Barbeiro, x => x.Atendimentos);

            return atendimentosPorBarbeiro;
        }

        public async Task<decimal[]> GetLucroDaSemanaAsync()
        {
            var lucroDaSemana = _context.Agendamentos
                .Where(a => a.DataHora >= DateTime.Now.AddDays(-7) && a.Status == StatusAgendamento.Concluido && a.PrecoTotal.HasValue)
                .AsEnumerable() // Traz os dados para a memória
                .GroupBy(a => a.DataHora.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(a => a.PrecoTotal.Value))
                .ToArray(); // `ToArray()` sem await

            return await Task.FromResult(lucroDaSemana);
        }

        public async Task<decimal[]> GetLucroDoMesAsync()
        {
            var lucroDoMes = _context.Agendamentos
                .Where(a => a.DataHora.Month == DateTime.Now.Month && a.Status == StatusAgendamento.Concluido && a.PrecoTotal.HasValue)
                .AsEnumerable()
                .GroupBy(a => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    a.DataHora,
                    CalendarWeekRule.FirstDay,
                    DayOfWeek.Monday))
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(a => a.PrecoTotal.Value))
                .ToArray();

            return await Task.FromResult(lucroDoMes); // Envolve o retorno em Task.FromResult para compatibilidade assíncrona
        }
    }
}
