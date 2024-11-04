using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infrastructure.Repositories
{
    public class RelatorioPersonalizadoRepository : IRelatorioPersonalizadoRepository
    {
        private readonly BarbeariaContext _context;

        public RelatorioPersonalizadoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task SalvarRelatorioPersonalizadoAsync(RelatorioPersonalizado relatorio)
        {
            await _context.RelatoriosPersonalizados.AddAsync(relatorio);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RelatorioPersonalizado>> ObterRelatoriosPorUsuarioAsync(int usuarioId)
        {
            return await _context.RelatoriosPersonalizados
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<RelatorioPersonalizado> ObterRelatorioPorIdAsync(int id)
        {
            return await _context.RelatoriosPersonalizados.FindAsync(id);
        }

        public async Task AtualizarRelatorioAsync(RelatorioPersonalizado relatorio)
        {
            _context.RelatoriosPersonalizados.Update(relatorio);
            await _context.SaveChangesAsync();
        }

        public async Task DeletarRelatorioAsync(int id)
        {
            var relatorio = await _context.RelatoriosPersonalizados.FindAsync(id);
            if (relatorio != null)
            {
                _context.RelatoriosPersonalizados.Remove(relatorio);
                await _context.SaveChangesAsync();
            }
        }
    }
}
