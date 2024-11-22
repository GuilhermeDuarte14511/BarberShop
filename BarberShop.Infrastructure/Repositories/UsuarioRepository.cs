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

        public async Task DeleteAsync(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Usuario>> ObterPorEmailOuTelefoneAsync(string email, string telefone)
        {
            // Busca por usuários com e-mail ou telefone correspondente
            return await _context.Usuarios
                .Where(u => u.Email == email || u.Telefone == telefone)
                .ToListAsync();
        }

    }
}
