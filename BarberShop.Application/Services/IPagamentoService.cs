using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IPagamentoService
    {
        Task<IEnumerable<Pagamento>> ObterPagamentosPorAgendamentoIdAsync(int agendamentoId);
        Task<IEnumerable<Pagamento>> ObterTodosPagamentosPorBarbeariaIdAsync(int barbeariaId);
        Task AdicionarPagamentoAsync(Pagamento pagamento);
        Task AtualizarPagamentoAsync(Pagamento pagamento);
        Task ExcluirPagamentoAsync(int id);
    }
}
