using BarberShop.Domain.Entities;

public interface IAgendamentoService
{
    Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeariaId, int barbeiroId, DateTime data, int duracaoTotal);
    Task<Agendamento> ObterAgendamentoPorIdAsync(int id);
    Task<IEnumerable<Servico>> ObterServicosAsync();
    Task<int> CriarAgendamentoAsync(int barbeariaId, int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds, string formaPagamento, decimal precoTotal);
    Task<IEnumerable<Pagamento>> ObterPagamentosPorAgendamentoIdAsync(int agendamentoId);
    Task<List<Agendamento>> ObterAgendamentosConcluidosAsync();
    Task AtualizarAgendamentoAsync(int id, Agendamento agendamentoAtualizado);
    Task<List<Agendamento>> ObterAgendamentosFuturosPorBarbeiroIdAsync(int barbeiroId);


}
