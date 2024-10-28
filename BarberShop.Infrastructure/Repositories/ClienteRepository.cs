using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        // Corrigir o AddAsync para adicionar Cliente
        public async Task<Cliente> AddAsync(Cliente entity)
        {
            await _context.Clientes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;  // Retorna o cliente adicionado
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<Cliente> GetByEmailOrPhoneAsync(string emailOrPhone)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Email == emailOrPhone || c.Telefone == emailOrPhone);
        }

        public async Task<Cliente> GetByIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task UpdateAsync(Cliente entity)
        {
            _context.Clientes.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
