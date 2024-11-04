using System;
using System.Threading.Tasks;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infrastructure.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        private readonly BarbeariaContext _context;

        public UsuarioRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Email == email)
                .SingleOrDefaultAsync();

            if (usuario != null)
            {
                usuario.CodigoValidacao = usuario.CodigoValidacao ?? string.Empty; // Se nulo, define como string vazia
                usuario.CodigoValidacaoExpiracao = usuario.CodigoValidacaoExpiracao ?? DateTime.Now; // Se nulo, define como data atual
            }

            return usuario;
        }

        public async Task UpdateCodigoVerificacaoAsync(int usuarioId, string codigoVerificacao, DateTime? expiracao)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.CodigoValidacao = codigoVerificacao;
                usuario.CodigoValidacaoExpiracao = expiracao;
                await _context.SaveChangesAsync();
            }
        }
    }
}
