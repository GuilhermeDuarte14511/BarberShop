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

            var agendamentosDaSemana = _context.Agendamentos
                .Where(a => a.DataHora >= startDate && a.DataHora <= DateTime.Now)
                .AsEnumerable()
                .GroupBy(a => a.DataHora.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToArray();

            return await Task.FromResult(agendamentosDaSemana);
        }

        public async Task<Dictionary<string, int>> GetServicosMaisSolicitadosAsync()
        {
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
                .AsEnumerable()
                .GroupBy(a => a.DataHora.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(a => a.PrecoTotal.Value))
                .ToArray();

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

            return await Task.FromResult(lucroDoMes);
        }

        public async Task<Dictionary<string, decimal>> GetCustomReportDataAsync(string reportType, int periodDays)
        {
            try
            {
                DateTime startDate = DateTime.Now.AddDays(-periodDays);

                switch (reportType)
                {
                    case "agendamentosPorStatus":
                        // Agrupa por Status como inteiro e converte para o nome do enum
                        return await _context.Agendamentos
                            .Where(a => a.DataHora >= startDate)
                            .GroupBy(a => a.Status) // Agrupa diretamente pelo valor do enum (int)
                            .ToDictionaryAsync(
                                g => ((StatusAgendamento)g.Key).ToString(), // Converte o valor para o nome do enum
                                g => (decimal)g.Count()
                            );

                    case "servicosMaisSolicitados":
                        return await _context.AgendamentoServicos
                            .Include(a => a.Servico)
                            .Where(a => a.Agendamento.DataHora >= startDate)
                            .GroupBy(a => a.Servico.Nome)
                            .ToDictionaryAsync(g => g.Key, g => (decimal)g.Count());

                    case "lucroPorFormaPagamento":
                        return await _context.Agendamentos
                            .Where(a => a.DataHora >= startDate && a.Status == StatusAgendamento.Concluido)
                            .GroupBy(a => a.FormaPagamento)
                            .ToDictionaryAsync(g => g.Key ?? "Indefinido", g => g.Sum(a => a.PrecoTotal ?? 0));

                    case "clientesFrequentes":
                        return await _context.Agendamentos
                            .Where(a => a.DataHora >= startDate)
                            .GroupBy(a => a.ClienteId)
                            .OrderByDescending(g => g.Count())
                            .Take(5)
                            .ToDictionaryAsync(g => g.Key.ToString(), g => (decimal)g.Count());

                    case "pagamentosPorStatus":
                        return await _context.Agendamentos
                            .Where(a => a.DataHora >= startDate)
                            .GroupBy(a => a.Status) // Agrupa pelo Status (int)
                            .ToDictionaryAsync(
                                g => ((StatusAgendamento)g.Key).ToString(), // Converte o valor do enum para string
                                g => g.Sum(a => a.PrecoTotal ?? 0)
                            );

                    case "servicosPorPreco":
                        return await _context.AgendamentoServicos
                            .Include(a => a.Servico)
                            .Where(a => a.Agendamento.DataHora >= startDate)
                            .GroupBy(a => new { a.Servico.Nome, a.Servico.Preco }) // Agrupa por nome e preço do serviço
                            .ToDictionaryAsync(
                                g => $"{g.Key.Nome} - R$ {g.Key.Preco.ToString("F2")}", // Chave composta por nome e preço formatado
                                g => (decimal)g.Count()
                            );


                    case "lucroPorPeriodo":
                        return await _context.Agendamentos
                            .Where(a => a.DataHora >= startDate && a.Status == StatusAgendamento.Concluido)
                            .GroupBy(a => a.DataHora.Date)
                            .ToDictionaryAsync(g => g.Key.ToShortDateString(), g => g.Sum(a => a.PrecoTotal ?? 0));

                    case "tempoMedioPorServico":
                        return await _context.AgendamentoServicos
                            .Include(a => a.Servico)
                            .Where(a => a.Agendamento.DataHora >= startDate)
                            .GroupBy(a => a.Servico.Nome)
                            .ToDictionaryAsync(g => g.Key, g => (decimal)g.Average(a => a.Servico.Duracao));

                    case "agendamentosCancelados":
                        return await _context.Agendamentos
                            .Where(a => a.DataHora >= startDate && a.Status == StatusAgendamento.Cancelado)
                            .GroupBy(a => a.Status)
                            .ToDictionaryAsync(
                                g => ((StatusAgendamento)g.Key).ToString(),
                                g => (decimal)g.Count()
                            );


                    default:
                        throw new ArgumentException("Tipo de relatório inválido");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar dados de relatório personalizado: {ex.Message}");
                throw;
            }
        }

        public async Task SaveChartPositionsAsync(List<GraficoPosicao> posicoes)
        {
            // Remover posições anteriores do usuário
            var usuarioId = posicoes.FirstOrDefault()?.UsuarioId;
            if (usuarioId != null)
            {
                var posicoesAntigas = _context.GraficoPosicao.Where(p => p.UsuarioId == usuarioId);
                _context.GraficoPosicao.RemoveRange(posicoesAntigas);
            }

            // Adicionar novas posições
            await _context.GraficoPosicao.AddRangeAsync(posicoes);
            await _context.SaveChangesAsync();
        }

        public async Task<List<GraficoPosicao>> GetChartPositionsAsync(int usuarioId)
        {
            return await _context.GraficoPosicao
                .Where(p => p.UsuarioId == usuarioId)
                .OrderBy(p => p.Posicao)
                .ToListAsync();
        }


    }
}
