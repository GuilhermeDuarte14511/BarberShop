using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IAgendamentoRepository : IRepository<Agendamento>
    {
        Task<IEnumerable<Agendamento>> GetByClienteIdWithServicosAsync(int clienteId, int? barbeariaId);
        Task<IEnumerable<Agendamento>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeariaId, int barbeiroId, DateTime date, int duracaoTotal, Dictionary<DayOfWeek, (TimeSpan abertura, TimeSpan fechamento)> horarioFuncionamento, HashSet<DateTime> feriados, List<(DateTime DataInicio, DateTime DataFim)> indisponibilidades);
        Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroIdAsync(int barbeiroId, DateTime data);
        Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEHorarioAsync(int barbeiroId, DateTime dataHoraInicio, DateTime dataHoraFim);
        Task AtualizarStatusPagamentoAsync(int agendamentoId, StatusPagamento statusPagamento, string paymentId = null);
        Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao);
        Task<Agendamento> GetDataAvaliacaoAsync(int agendamentoId);
        Task<IEnumerable<Agendamento>> GetAgendamentosPorPeriodoAsync(int barbeariaId, DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<Agendamento>> GetAgendamentosPorBarbeariaAsync(int barbeariaId);
        Task<Agendamento> GetByIdAndBarbeariaIdAsync(int id, int barbeariaId);
        Task<IEnumerable<Agendamento>> ObterAgendamentosConcluidosSemEmailAsync();
        Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEBarbeariaAsync(int barbeiroId, int barbeariaId, int? agendamentoId = null);
        Task<IEnumerable<Agendamento>> FiltrarAgendamentosAsync(int? barbeiroId, int barbeariaId, string clienteNome = null, DateTime? dataInicio = null, DateTime? dataFim = null, string formaPagamento = null,
                                                                StatusAgendamento? status = null, StatusPagamento? statusPagamento = null, string barbeiroNome = null, int? agendamentoId = null);

        Task<Agendamento> ObterAgendamentoCompletoPorIdAsync(int id);

    }
}
