using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infrastructure.Repositories
{
    public class BarbeariaRepository : Repository<Barbearia>, IBarbeariaRepository
    {
        private readonly BarbeariaContext _context;

        public BarbeariaRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Barbearia> GetByUrlSlugAsync(string urlSlug)
        {
            return await _context.Barbearias.FirstOrDefaultAsync(b => b.UrlSlug == urlSlug);
        }

        public async Task<IEnumerable<Barbearia>> ObterTodasAtivasAsync()
        {
            return await _context.Barbearias
                .Where(b => b.Status == true)
                .ToListAsync();
        }

        public async Task DeleteAsync(Barbearia barbearia)
        {
            _context.Barbearias.Remove(barbearia);
            await _context.SaveChangesAsync();
        }

        // Implementação do novo método para verificar existência de UrlSlug
        public async Task<bool> ExistsByUrlSlugAsync(string urlSlug)
        {
            return await _context.Barbearias.AnyAsync(b => b.UrlSlug == urlSlug);
        }

        public async Task AtualizarLogoAsync(int barbeariaId, byte[] logo)
        {
            var barbearia = await _context.Barbearias.FindAsync(barbeariaId);
            if (barbearia != null)
            {
                // Preserva o Endereço atual com o número, caso ele já esteja no banco de dados
                var enderecoAtual = barbearia.Endereco;

                // Atualiza apenas a Logo
                barbearia.Logo = logo;

                // Marca a propriedade Logo como modificada
                _context.Entry(barbearia).Property(b => b.Logo).IsModified = true;

                // Força o Endereço a manter o valor original (com o número, caso já esteja)
                _context.Entry(barbearia).Property(b => b.Endereco).OriginalValue = enderecoAtual;

                await _context.SaveChangesAsync();
            }
        }


        public async Task<byte[]> ObterLogoAsync(int barbeariaId)
        {
            return await _context.Barbearias
                .Where(b => b.BarbeariaId == barbeariaId)
                .Select(b => b.Logo)
                .FirstOrDefaultAsync();
        }

        // Implementação do novo método para obter o AccountIdStripe da barbearia
        public async Task<string> GetAccountIdStripeByIdAsync(int barbeariaId)
        {
            return await _context.Barbearias
                .Where(b => b.BarbeariaId == barbeariaId)
                .Select(b => b.AccountIdStripe)
                .FirstOrDefaultAsync();
        }
    }
}
