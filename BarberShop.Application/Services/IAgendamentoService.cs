using BarberShop.Domain.Entities;

public interface IAgendamentoService
{
    Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, DateTime data, int duracaoTotal);
    Task<Agendamento> ObterAgendamentoPorIdAsync(int id);
    Task<IEnumerable<Servico>> ObterServicosAsync();

    Task<int> CriarAgendamentoAsync(int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds, string formaPagamento, decimal precoTotal, StatusPagamento statusPagamento = StatusPagamento.Pendente, string paymentId = null);
}
