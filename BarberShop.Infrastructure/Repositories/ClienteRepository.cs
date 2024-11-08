using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
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

        // Método para adicionar um novo cliente
        public async Task<Cliente> AddAsync(Cliente entity)
        {
            await _context.Clientes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
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

        public async Task<Cliente> GetByEmailOrPhoneAsync(string email, string phone, int barbeariaId)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => (c.Email == email || c.Telefone == phone) && c.BarbeariaId == barbeariaId);
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

        // Novo método para atualizar apenas o código de verificação e a data de expiração
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
    }
}
