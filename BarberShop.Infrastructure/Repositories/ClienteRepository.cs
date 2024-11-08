using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly BarbeariaContext _context;

        public ClienteRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<Cliente> AddAsync(Cliente entity)
        {
            await _context.Clientes.AddAsync(entity);
            return entity; // Não chama SaveChangesAsync aqui
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                // Não chama SaveChangesAsync aqui
            }
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> GetAllByBarbeariaIdAsync(int barbeariaId)
        {
            return await _context.Clientes
                .Where(c => c.BarbeariaId == barbeariaId)
                .ToListAsync();
        }

        public async Task<Cliente> GetByIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<Cliente> GetByIdAndBarbeariaIdAsync(int clienteId, int barbeariaId)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.BarbeariaId == barbeariaId);
        }

        public async Task<Cliente> GetByEmailOrPhoneAsync(string email, string phone, int barbeariaId)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => (c.Email == email || c.Telefone == phone) && c.BarbeariaId == barbeariaId);
        }

        public async Task UpdateAsync(Cliente entity)
        {
            _context.Clientes.Update(entity);
            // Não chama SaveChangesAsync aqui
        }

        public async Task UpdateCodigoVerificacaoAsync(int clienteId, string codigoVerificacao, DateTime? expiracao)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente != null)
            {
                cliente.CodigoValidacao = codigoVerificacao;
                cliente.CodigoValidacaoExpiracao = expiracao;

                _context.Entry(cliente).Property(c => c.CodigoValidacao).IsModified = true;
                _context.Entry(cliente).Property(c => c.CodigoValidacaoExpiracao).IsModified = true;

                await _context.SaveChangesAsync();
            }
        }

        // Implementação do SaveChangesAsync
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
