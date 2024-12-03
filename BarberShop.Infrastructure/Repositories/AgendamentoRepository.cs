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
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Include(a => a.AgendamentoServicos)
                    .ThenInclude(asg => asg.Servico)
                .Include(a => a.Pagamento) // Inclui o pagamento do agendamento
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
                .Include(a => a.Barbeiro) // Inclui o barbeiro responsável
                .Include(a => a.AgendamentoServicos) // Inclui os serviços do agendamento
                    .ThenInclude(asg => asg.Servico) // Inclui os detalhes dos serviços
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
                .Include(a => a.Pagamento) // Inclui o pagamento do agendamento
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

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeariaId, int barbeiroId, DateTime dataVisualizacao, int duracaoTotal, Dictionary<DayOfWeek, (TimeSpan abertura, TimeSpan fechamento)> horarioFuncionamento, HashSet<DateTime> feriados,
        List<(DateTime DataInicio, DateTime DataFim)> indisponibilidades)
        {
            var horariosDisponiveis = new List<DateTime>();
            DateTime dataInicio = dataVisualizacao.Date;
            DateTime dataFim = dataInicio.AddDays(14).Date; // Horários para 14 dias

            Console.WriteLine($"Gerando horários disponíveis de {dataInicio} até {dataFim}");

            for (DateTime dataAtual = dataInicio; dataAtual <= dataFim; dataAtual = dataAtual.AddDays(1))
            {
                // Ignorar feriados
                if (feriados.Contains(dataAtual.Date))
                {
                    Console.WriteLine($"Feriado em {dataAtual:dd/MM/yyyy}. Pulando...");
                    continue;
                }

                // Verificar horário de funcionamento do dia
                if (!horarioFuncionamento.TryGetValue(dataAtual.DayOfWeek, out var funcionamentoDia))
                {
                    Console.WriteLine($"Barbearia fechada em {dataAtual:dd/MM/yyyy}. Pulando...");
                    continue;
                }

                DateTime horarioAbertura = dataAtual.Date.Add(funcionamentoDia.abertura);
                DateTime horarioFechamento = dataAtual.Date.Add(funcionamentoDia.fechamento);

                // Ajustar para o próximo horário válido, considerando o "agora"
                if (dataAtual.Date == DateTime.Now.Date && horarioAbertura < DateTime.Now)
                {
                    horarioAbertura = DateTime.Now.AddMinutes(duracaoTotal); // Ajustar para o próximo horário
                }

                // Verificar indisponibilidades
                if (indisponibilidades.Any(i => i.DataInicio.Date <= dataAtual.Date && i.DataFim.Date >= dataAtual.Date))
                {
                    Console.WriteLine($"Indisponibilidade no dia {dataAtual:dd/MM/yyyy}. Pulando...");
                    continue;
                }

                // Obter os agendamentos do barbeiro no dia
                var agendamentosDoDia = await _context.Agendamentos
                    .Where(a => a.BarbeiroId == barbeiroId && a.DataHora.Date == dataAtual.Date)
                    .OrderBy(a => a.DataHora)
                    .ToListAsync();

                DateTime horarioAtual = horarioAbertura;

                foreach (var agendamento in agendamentosDoDia)
                {
                    DateTime inicioAgendamento = agendamento.DataHora;
                    DateTime fimAgendamento = inicioAgendamento.AddMinutes(agendamento.DuracaoTotal ?? 0);

                    // Adicionar horários disponíveis antes do próximo agendamento
                    while (horarioAtual.AddMinutes(duracaoTotal) <= inicioAgendamento && horarioAtual.AddMinutes(duracaoTotal) <= horarioFechamento)
                    {
                        horariosDisponiveis.Add(horarioAtual);
                        Console.WriteLine($"Horário disponível: {horarioAtual:HH:mm} - {horarioAtual.AddMinutes(duracaoTotal):HH:mm}");
                        horarioAtual = horarioAtual.AddMinutes(duracaoTotal);
                    }

                    // Ajustar o horário atual para o fim do agendamento
                    if (horarioAtual < fimAgendamento)
                    {
                        horarioAtual = fimAgendamento;
                    }
                }

                // Adicionar horários disponíveis após o último agendamento até o horário de fechamento
                while (horarioAtual.AddMinutes(duracaoTotal) <= horarioFechamento)
                {
                    horariosDisponiveis.Add(horarioAtual);
                    Console.WriteLine($"Horário disponível: {horarioAtual:HH:mm} - {horarioAtual.AddMinutes(duracaoTotal):HH:mm}");
                    horarioAtual = horarioAtual.AddMinutes(duracaoTotal);
                }
            }

            return horariosDisponiveis;
        }








        // Implementação do método ObterAgendamentosPorBarbeiroIdAsync
        public async Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroIdAsync(int barbeiroId, DateTime data)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbearia)
                .Where(a => a.BarbeiroId == barbeiroId && a.DataHora >= data)
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


        public async Task<IEnumerable<Agendamento>> ObterAgendamentosConcluidosSemEmailAsync()
        {
            return await _context.Agendamentos
                .Where(a => a.Status == StatusAgendamento.Concluido &&
                            (a.EmailEnviado == null || a.EmailEnviado == false))
                .Include(a => a.Cliente)
                .Include(a => a.Barbearia)
                .ToListAsync();
        }


        public async Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEBarbeariaAsync(int barbeiroId, int barbeariaId, int? agendamentoId = null)
        {
            var query = _context.Agendamentos
                .AsNoTracking()
                .Include(a => a.Cliente)
                .Include(a => a.AgendamentoServicos)
                .Include(a => a.Pagamento)
                .Where(a => a.BarbeiroId == barbeiroId && a.BarbeariaId == barbeariaId);

            if (agendamentoId.HasValue)
            {
                query = query.Where(a => a.AgendamentoId == agendamentoId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Agendamento>> FiltrarAgendamentosAsync(int? barbeiroId, int barbeariaId, string clienteNome = null, DateTime? dataInicio = null, DateTime? dataFim = null, string formaPagamento = null, StatusAgendamento? status = null, StatusPagamento? statusPagamento = null,
                                                                             string barbeiroNome = null, int? agendamentoId = null)
        {
            var query = _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Include(a => a.Pagamento)
                .Where(a => a.BarbeariaId == barbeariaId)
                .AsQueryable();

            if (agendamentoId.HasValue)
            {
                query = query.Where(a => a.AgendamentoId == agendamentoId.Value);
                return await query.ToListAsync(); // Retorna imediatamente quando agendamentoId é fornecido
            }

            if (!string.IsNullOrEmpty(clienteNome))
            {
                query = query.Where(a => a.Cliente.Nome.Contains(clienteNome));
            }

            if (barbeiroId.HasValue)
            {
                query = query.Where(a => a.BarbeiroId == barbeiroId.Value);
            }

            if (!string.IsNullOrEmpty(barbeiroNome))
            {
                query = query.Where(a => a.Barbeiro.Nome.Contains(barbeiroNome));
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(a => a.DataHora >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(a => a.DataHora <= dataFim.Value);
            }

            if (!string.IsNullOrEmpty(formaPagamento))
            {
                query = query.Where(a => a.FormaPagamento == formaPagamento);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (statusPagamento.HasValue)
            {
                query = query.Where(a => a.Pagamento.StatusPagamento == statusPagamento.Value);
            }

            return await query.ToListAsync();
        }



        public async Task<Agendamento> ObterAgendamentoCompletoPorIdAsync(int id)
        {
            return await _context.Agendamentos
                .AsNoTracking()
                .Include(a => a.Cliente) // Inclui os dados do cliente
                .Include(a => a.AgendamentoServicos) // Inclui os serviços do agendamento
                    .ThenInclude(agendamentoServico => agendamentoServico.Servico) // Inclui os detalhes de cada serviço
                .Include(a => a.Pagamento) // Inclui os dados do pagamento
                .Include(a => a.Barbeiro) // Inclui os dados do barbeiro
                .FirstOrDefaultAsync(a => a.AgendamentoId == id); // Filtra pelo ID específico
        }

    }
}
