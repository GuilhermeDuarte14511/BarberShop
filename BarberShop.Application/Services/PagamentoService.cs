using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class PagamentoService : IPagamentoService
    {
        private readonly IPagamentoRepository _pagamentoRepository;

        public PagamentoService(IPagamentoRepository pagamentoRepository)
        {
            _pagamentoRepository = pagamentoRepository;
        }

        public async Task<IEnumerable<Pagamento>> ObterPagamentosPorAgendamentoIdAsync(int agendamentoId)
        {
            return await _pagamentoRepository.GetPagamentosPorAgendamentoIdAsync(agendamentoId);
        }

        public async Task<IEnumerable<Pagamento>> ObterTodosPagamentosPorBarbeariaIdAsync(int barbeariaId)
        {
            return await _pagamentoRepository.GetAllPagamentosByBarbeariaIdAsync(barbeariaId);
        }

        public async Task AdicionarPagamentoAsync(Pagamento pagamento)
        {
            await _pagamentoRepository.AddAsync(pagamento);
        }

        public async Task AtualizarPagamentoAsync(Pagamento pagamento)
        {
            await _pagamentoRepository.UpdateAsync(pagamento);
        }

        public async Task ExcluirPagamentoAsync(int id)
        {
            await _pagamentoRepository.DeleteAsync(id);
        }
    }
}
