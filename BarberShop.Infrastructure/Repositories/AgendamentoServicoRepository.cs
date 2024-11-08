using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; // Adicione esta linha
using System.Collections.Generic;
using System.Threading.Tasks;

public class AgendamentoServicoRepository : IRepository<AgendamentoServico>
{
    private readonly BarbeariaContext _context;

    public AgendamentoServicoRepository(BarbeariaContext context)
    {
        _context = context;
    }

    public async Task<AgendamentoServico> AddAsync(AgendamentoServico entity)
    {
        await _context.AgendamentoServicos.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;  // Retorna o agendamento-serviço adicionado
    }

    public async Task DeleteAsync(int id)
    {
        var agendamentoServico = await _context.AgendamentoServicos.FindAsync(id);
        if (agendamentoServico != null)
        {
            _context.AgendamentoServicos.Remove(agendamentoServico);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AgendamentoServico>> GetAllAsync()
    {
        return await _context.AgendamentoServicos.ToListAsync(); // Aqui utiliza ToListAsync
    }

    public async Task<AgendamentoServico> GetByIdAsync(int id)
    {
        return await _context.AgendamentoServicos.FindAsync(id);
    }

    public async Task UpdateAsync(AgendamentoServico entity)
    {
        _context.AgendamentoServicos.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Implementação do SaveChangesAsync
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync(); // Retorna o número de entradas afetadas
    }
}
