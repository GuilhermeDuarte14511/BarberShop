using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly BarbeariaContext _context;

        public PaymentRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PagamentoAssinatura pagamento)
        {
            await _context.PagamentosAssinaturasSite.AddAsync(pagamento);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
