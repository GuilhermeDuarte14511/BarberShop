using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly BarbeariaContext _context;

    public AvaliacaoRepository(BarbeariaContext context)
    {
        _context = context;
    }

    public async Task<Avaliacao> AddAsync(Avaliacao entity)
    {
        await _context.Avaliacao.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var avaliacao = await _context.Avaliacao.FindAsync(id);
        if (avaliacao != null)
        {
            _context.Avaliacao.Remove(avaliacao);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Avaliacao>> GetAllAsync()
    {
        return await _context.Avaliacao.ToListAsync();
    }

    public async Task<Avaliacao> GetByIdAsync(int id)
    {
        return await _context.Avaliacao.FindAsync(id);
    }

    public async Task<Avaliacao> ObterAvaliacaoPorAgendamentoIdAsync(int agendamentoId)
    {
        return await _context.Avaliacao
            .Include(a => a.Agendamento)
                .ThenInclude(ag => ag.Barbeiro) // Inclui informações do barbeiro
            .Include(a => a.Agendamento)
                .ThenInclude(ag => ag.AgendamentoServicos)
                    .ThenInclude(ags => ags.Servico)
            .FirstOrDefaultAsync(a => a.AgendamentoId == agendamentoId);
    }



    public async Task UpdateAsync(Avaliacao entity)
    {
        _context.Avaliacao.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Implementação do SaveChangesAsync
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync(); // Retorna o número de entradas afetadas
    }

    public async Task<Avaliacao> AdicionarAvaliacaoAsync(Avaliacao avaliacao)
    {
        var avaliacaoAdicionada = await _context.Avaliacao.AddAsync(avaliacao);
        await _context.SaveChangesAsync(); // Persiste no banco
        return avaliacaoAdicionada.Entity; // Retorna a entidade persistida
    }
}
