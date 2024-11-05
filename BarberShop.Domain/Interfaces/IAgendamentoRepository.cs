using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;

public interface IAgendamentoRepository : IRepository<Agendamento>
{
    Task<IEnumerable<Agendamento>> GetByClienteIdAsync(int clienteId);
    Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int barbeiroId, DateTime date, int duracaoTotal);
    Task<IEnumerable<Agendamento>> GetByClienteIdWithServicosAsync(int clienteId); // Alterado para retornar AgendamentoDto
    Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroIdAsync(int barbeiroId, DateTime data);
    Task<IEnumerable<Agendamento>> ObterAgendamentosPorBarbeiroEHorarioAsync(int barbeiroId, DateTime dataHoraInicio, DateTime dataHoraFim);
    Task AtualizarStatusPagamentoAsync(int agendamentoId, StatusPagamento statusPagamento, string paymentId = null);

    // Novo método para verificar disponibilidade de horário
    Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao);

    Task<Agendamento> GetDataAvaliacaoAsync(int agendamentoId); // Adicione esta linha

}
