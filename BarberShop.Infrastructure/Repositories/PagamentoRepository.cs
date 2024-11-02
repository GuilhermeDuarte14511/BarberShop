using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BarberShop.Infrastructure.Repositories
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly BarbeariaContext _context;

        public PagamentoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<Pagamento> AddAsync(Pagamento entity)
        {
            await _context.Pagamentos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var pagamento = await _context.Pagamentos.FindAsync(id);
            if (pagamento != null)
            {
                _context.Pagamentos.Remove(pagamento);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Pagamento>> GetAllAsync()
        {
            return await _context.Pagamentos.ToListAsync();
        }

        public async Task<Pagamento> GetByIdAsync(int id)
        {
            return await _context.Pagamentos.FindAsync(id);
        }

        public async Task<IEnumerable<Pagamento>> GetPagamentosPorAgendamentoIdAsync(int agendamentoId)
        {
            return await _context.Pagamentos
                .Where(p => p.AgendamentoId == agendamentoId)
                .Select(p => new Pagamento
                {
                    PagamentoId = p.PagamentoId,
                    AgendamentoId = p.AgendamentoId,
                    ClienteId = p.ClienteId,
                    ValorPago = p.ValorPago ?? 0m, // Define 0 como padrão para valores nulos
                    StatusPagamento = p.StatusPagamento,
                    PaymentId = p.PaymentId ?? string.Empty, // Define string vazia como padrão para valores nulos
                    DataPagamento = p.DataPagamento // Permite nulo, mas sem erro ao acessar diretamente
                })
                .ToListAsync();
        }



        public async Task UpdateAsync(Pagamento entity)
        {
            _context.Pagamentos.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
