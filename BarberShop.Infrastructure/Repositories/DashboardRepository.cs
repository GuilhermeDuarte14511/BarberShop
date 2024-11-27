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

        public async Task<int[]> GetAgendamentosPorSemanaAsync(int barbeariaId, int? barbeiroId = null)
        {
            var hoje = DateTime.Now.Date;
            var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek + (int)DayOfWeek.Monday); // Segunda-feira
            var fimSemana = inicioSemana.AddDays(6).Date; // Domingo

            var query = _context.Agendamentos
                .Where(a => a.DataHora.Date >= inicioSemana && a.DataHora.Date <= fimSemana && a.BarbeariaId == barbeariaId);

            // Aplica o filtro pelo barbeiro, caso fornecido
            if (barbeiroId.HasValue)
            {
                query = query.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            // Executa a consulta no banco de dados e traz os dados para memória
            var agendamentos = await query
                .Select(a => new { a.DataHora })
                .ToListAsync();

            // Agrupa os agendamentos por dia da semana no lado do cliente
            var agendamentosDaSemana = agendamentos
                .GroupBy(a => a.DataHora.DayOfWeek)
                .OrderBy(g => g.Key) // Ordena os dias da semana
                .Select(g => g.Count()) // Conta os agendamentos em cada dia
                .ToArray(); // Converte para array

            // Retorna o resultado
            return agendamentosDaSemana;
        }




        public async Task<Dictionary<string, int>> GetServicosMaisSolicitadosAsync(int barbeariaId, int? barbeiroId = null)
        {
            // Consulta base para buscar os serviços relacionados aos agendamentos da barbearia
            var query = _context.AgendamentoServicos
                .Include(ag => ag.Servico)
                .Where(ag => ag.Agendamento.BarbeariaId == barbeariaId);

            // Aplica o filtro pelo barbeiro, caso fornecido
            if (barbeiroId.HasValue)
            {
                query = query.Where(ag => ag.Agendamento.BarbeiroId == barbeiroId.Value);
            }

            // Executa a consulta e realiza o agrupamento
            var agendamentoServicos = await query.ToListAsync();

            // Agrupa os serviços por nome, ordena pelos mais solicitados e pega os 3 principais
            var servicosMaisSolicitados = agendamentoServicos
                .GroupBy(ag => ag.Servico.Nome)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .ToDictionary(g => g.Key, g => g.Count());

            return servicosMaisSolicitados;
        }


        public async Task<Dictionary<string, decimal>> GetLucroPorBarbeiroAsync(int barbeariaId, int? barbeiroId = null)
        {
            // Consulta base para buscar agendamentos concluídos com preço na barbearia
            var query = _context.Agendamentos
                .Where(a => a.Status == StatusAgendamento.Concluido && a.PrecoTotal.HasValue && a.BarbeariaId == barbeariaId);

            // Aplica o filtro pelo barbeiro, caso fornecido
            if (barbeiroId.HasValue)
            {
                query = query.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            // Agrupa os agendamentos por barbeiro e soma os lucros
            var lucroPorBarbeiro = await query
                .GroupBy(a => a.Barbeiro.Nome)
                .Select(g => new { Barbeiro = g.Key, Lucro = g.Sum(a => a.PrecoTotal.Value) })
                .ToDictionaryAsync(x => x.Barbeiro, x => x.Lucro);

            return lucroPorBarbeiro;
        }


        public async Task<Dictionary<string, int>> GetAtendimentosPorBarbeiroAsync(int barbeariaId, int? barbeiroId = null)
        {
            // Consulta base para buscar agendamentos da barbearia
            var query = _context.Agendamentos
                .Where(a => a.BarbeariaId == barbeariaId);

            // Aplica o filtro pelo barbeiro, caso fornecido
            if (barbeiroId.HasValue)
            {
                query = query.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            // Agrupa os agendamentos por barbeiro e conta o número de atendimentos
            var atendimentosPorBarbeiro = await query
                .GroupBy(a => a.Barbeiro.Nome)
                .Select(g => new { Barbeiro = g.Key, Atendimentos = g.Count() })
                .ToDictionaryAsync(x => x.Barbeiro, x => x.Atendimentos);

            return atendimentosPorBarbeiro;
        }


        public async Task<decimal[]> GetLucroDaSemanaAsync(int barbeariaId, int? barbeiroId = null)
        {
            // Determina o início e o fim da semana atual (segunda a domingo)
            var hoje = DateTime.Now.Date;
            var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek + (int)DayOfWeek.Monday); // Segunda-feira
            var fimSemana = inicioSemana.AddDays(6).Date; // Domingo

            // Consulta base para buscar agendamentos concluídos com lucro na barbearia
            var query = _context.Agendamentos
                .Where(a => a.DataHora.Date >= inicioSemana
                            && a.DataHora.Date <= fimSemana
                            && a.Status == StatusAgendamento.Concluido
                            && a.PrecoTotal.HasValue
                            && a.BarbeariaId == barbeariaId);

            // Aplica o filtro pelo barbeiro, caso fornecido
            if (barbeiroId.HasValue)
            {
                query = query.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            // Carrega os dados relevantes para a memória
            var agendamentos = await query
                .Select(a => new { a.DataHora, a.PrecoTotal })
                .ToListAsync();

            // Agrupa os lucros por dia da semana no lado do cliente
            var lucroDaSemana = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(dia =>
                    agendamentos
                        .Where(a => a.DataHora.DayOfWeek == dia)
                        .Sum(a => a.PrecoTotal ?? 0)) // Calcula o total do dia, retorna 0 se não houver
                .ToArray();

            // Retorna o resultado
            return lucroDaSemana;
        }

        public async Task<Dictionary<string, decimal>> GetLucroDoMesAsync(int barbeariaId, int? barbeiroId = null)
        {
            // Obtém a data atual do sistema
            var dataAtual = DateTime.Now;

            // Define o primeiro e o último dia do mês atual
            var primeiroDiaDoMes = new DateTime(dataAtual.Year, dataAtual.Month, 1);
            var ultimoDiaDoMes = primeiroDiaDoMes.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // Consulta base para buscar agendamentos concluídos no mês atual
            var query = _context.Agendamentos
                .Where(a => a.DataHora >= primeiroDiaDoMes
                            && a.DataHora <= ultimoDiaDoMes
                            && a.Status == StatusAgendamento.Concluido
                            && a.PrecoTotal.HasValue
                            && a.BarbeariaId == barbeariaId);

            // Aplica o filtro pelo barbeiro, caso fornecido
            if (barbeiroId.HasValue)
            {
                query = query.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            var agendamentos = await query
                .Select(a => new { a.DataHora, a.PrecoTotal })
                .ToListAsync();

            // Calcula o número total de semanas no mês atual
            int totalSemanas = (int)Math.Ceiling((double)ultimoDiaDoMes.Day / 7);

            // Agrupa os lucros por semanas dentro do mês atual
            var lucroPorSemana = agendamentos
                .GroupBy(a =>
                {
                    // Calcula a semana do mês em que a DataHora do agendamento se encontra
                    var diferencaDias = (a.DataHora - primeiroDiaDoMes).Days;
                    return (diferencaDias / 7) + 1; // Calcula a semana do mês atual (1, 2, 3, 4, 5...)
                })
                .ToDictionary(
                    g => g.Key, // Chave representando a semana
                    g => g.Sum(a => a.PrecoTotal.Value) // Soma o lucro por semana
                );

            // Preenche as semanas que não possuem agendamentos com lucro 0
            var resultadoCompleto = new Dictionary<string, decimal>();
            for (int i = 1; i <= totalSemanas; i++)
            {
                resultadoCompleto[$"Semana {i}"] = lucroPorSemana.ContainsKey(i) ? lucroPorSemana[i] : 0m;
            }

            return resultadoCompleto;
        }




        public async Task<Dictionary<string, decimal>> GetCustomReportDataAsync(int barbeariaId, string reportType, int periodDays, int? barbeiroId = null)
        {
            DateTime startDate = DateTime.Now.AddDays(-periodDays);

            IQueryable<Agendamento> agendamentoQuery = _context.Agendamentos
                .Where(a => a.DataHora >= startDate && a.BarbeariaId == barbeariaId);

            if (barbeiroId.HasValue)
            {
                agendamentoQuery = agendamentoQuery.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            switch (reportType)
            {
                case "agendamentosPorStatus":
                    return await agendamentoQuery
                        .GroupBy(a => a.Status)
                        .ToDictionaryAsync(
                            g => ((StatusAgendamento)g.Key).ToString(),
                            g => (decimal)g.Count()
                        );

                case "servicosMaisSolicitados":
                    var servicoQuery = _context.AgendamentoServicos
                        .Include(a => a.Servico)
                        .Where(a => a.Agendamento.DataHora >= startDate && a.Agendamento.BarbeariaId == barbeariaId);

                    if (barbeiroId.HasValue)
                    {
                        servicoQuery = servicoQuery.Where(a => a.Agendamento.BarbeiroId == barbeiroId.Value);
                    }

                    return await servicoQuery
                        .GroupBy(a => a.Servico.Nome)
                        .ToDictionaryAsync(g => g.Key, g => (decimal)g.Count());

                case "lucroPorFormaPagamento":
                    return await agendamentoQuery
                        .Where(a => a.Status == StatusAgendamento.Concluido)
                        .GroupBy(a => a.FormaPagamento)
                        .ToDictionaryAsync(
                            g => g.Key ?? "Indefinido",
                            g => g.Sum(a => a.PrecoTotal ?? 0)
                        );

                case "clientesFrequentes":
                    return await agendamentoQuery
                        .GroupBy(a => a.ClienteId)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .ToDictionaryAsync(
                            g => g.Key.ToString(),
                            g => (decimal)g.Count()
                        );

                case "pagamentosPorStatus":
                    return await agendamentoQuery
                        .GroupBy(a => a.Status)
                        .ToDictionaryAsync(
                            g => ((StatusAgendamento)g.Key).ToString(),
                            g => g.Sum(a => a.PrecoTotal ?? 0)
                        );

                case "servicosPorPreco":
                    var servicoPrecoQuery = _context.AgendamentoServicos
                        .Include(a => a.Servico)
                        .Where(a => a.Agendamento.DataHora >= startDate && a.Agendamento.BarbeariaId == barbeariaId);

                    if (barbeiroId.HasValue)
                    {
                        servicoPrecoQuery = servicoPrecoQuery.Where(a => a.Agendamento.BarbeiroId == barbeiroId.Value);
                    }

                    return await servicoPrecoQuery
                        .GroupBy(a => new { a.Servico.Nome, a.Servico.Preco })
                        .ToDictionaryAsync(
                            g => $"{g.Key.Nome} - R$ {g.Key.Preco.ToString("F2")}",
                            g => (decimal)g.Count()
                        );

                case "lucroPorPeriodo":
                    return await agendamentoQuery
                        .Where(a => a.Status == StatusAgendamento.Concluido)
                        .GroupBy(a => a.DataHora.Date)
                        .ToDictionaryAsync(
                            g => g.Key.ToShortDateString(),
                            g => g.Sum(a => a.PrecoTotal ?? 0)
                        );

                case "tempoMedioPorServico":
                    var servicoTempoQuery = _context.AgendamentoServicos
                        .Include(a => a.Servico)
                        .Where(a => a.Agendamento.DataHora >= startDate && a.Agendamento.BarbeariaId == barbeariaId);

                    if (barbeiroId.HasValue)
                    {
                        servicoTempoQuery = servicoTempoQuery.Where(a => a.Agendamento.BarbeiroId == barbeiroId.Value);
                    }

                    return await servicoTempoQuery
                        .GroupBy(a => a.Servico.Nome)
                        .ToDictionaryAsync(
                            g => g.Key,
                            g => (decimal)g.Average(a => a.Servico.Duracao)
                        );

                case "agendamentosCancelados":
                    return await agendamentoQuery
                        .Where(a => a.Status == StatusAgendamento.Cancelado)
                        .GroupBy(a => a.Status)
                        .ToDictionaryAsync(
                            g => ((StatusAgendamento)g.Key).ToString(),
                            g => (decimal)g.Count()
                        );

                default:
                    throw new ArgumentException("Tipo de relatório inválido");
            }
        }


        public async Task SaveChartPositionsAsync(List<GraficoPosicao> posicoes)
        {
            var usuarioId = posicoes.FirstOrDefault()?.UsuarioId;
            if (usuarioId != null)
            {
                var posicoesAntigas = _context.GraficoPosicao.Where(p => p.UsuarioId == usuarioId);
                _context.GraficoPosicao.RemoveRange(posicoesAntigas);
            }

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
