using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class OnboardingProgressoRepository : IOnboardingProgressoRepository
    {
        private readonly BarbeariaContext _context;

        public OnboardingProgressoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<bool> VerificarProgressoAsync(int usuarioId, string tela)
        {
            return await _context.OnboardingProgresso
                .AnyAsync(o => o.UsuarioId == usuarioId && o.Tela == tela && o.Concluido);
        }

        public async Task SalvarProgressoAsync(OnboardingProgresso progresso)
        {
            var existente = await _context.OnboardingProgresso
                .FirstOrDefaultAsync(o => o.UsuarioId == progresso.UsuarioId && o.Tela == progresso.Tela);

            if (existente == null)
            {
                await _context.OnboardingProgresso.AddAsync(progresso);
            }
            else
            {
                existente.Concluido = true;
                existente.DataConclusao = progresso.DataConclusao;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<OnboardingProgresso>> ObterTodosAsync()
        {
            return await _context.OnboardingProgresso.ToListAsync();
        }

        public async Task<List<string>> ObterTelasPendentesAsync(int usuarioId)
        {
            return await _context.OnboardingProgresso
                .Where(o => o.UsuarioId == usuarioId && !o.Concluido)
                .Select(o => o.Tela)
                .ToListAsync();
        }
    }
}
