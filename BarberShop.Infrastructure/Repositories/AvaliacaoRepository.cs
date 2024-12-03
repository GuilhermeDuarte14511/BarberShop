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
        await _context.SaveChangesAsync();
        return avaliacaoAdicionada.Entity;
    }

    public async Task<IEnumerable<Avaliacao>> ObterAvaliacoesPorBarbeiroIdAsync(int barbeiroId, int? avaliacaoId = null)
    {
        var query = _context.Avaliacao
            .Include(a => a.Agendamento)
            .Where(a => a.Agendamento.BarbeiroId == barbeiroId);

        // Aplica o filtro por avaliacaoId, se fornecido
        if (avaliacaoId.HasValue)
        {
            query = query.Where(a => a.AvaliacaoId == avaliacaoId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Avaliacao>> ObterAvaliacoesFiltradasAsync(int? barbeariaId = null, int? barbeiroId = null, string? dataInicio = null, string? dataFim = null, int? notaServico = null, int? notaBarbeiro = null, string? observacao = null)
    {
        var query = _context.Avaliacao
          .AsNoTracking()
          .Include(a => a.Agendamento)
          .ThenInclude(ag => ag.Barbeiro)
          .Include(a => a.Agendamento.Cliente) 
          .AsQueryable();

        if (barbeariaId.HasValue)
        {
            query = query.Where(a => a.Agendamento.BarbeariaId == barbeariaId.Value);
        }

        if (barbeiroId.HasValue)
        {
            query = query.Where(a => a.Agendamento.BarbeiroId == barbeiroId.Value);
        }

        if (!string.IsNullOrEmpty(dataInicio) && DateTime.TryParse(dataInicio, out var dataInicioParsed))
        {
            query = query.Where(a => a.DataAvaliado >= dataInicioParsed);
        }

        if (!string.IsNullOrEmpty(dataFim) && DateTime.TryParse(dataFim, out var dataFimParsed))
        {
            query = query.Where(a => a.DataAvaliado <= dataFimParsed);
        }

        if (notaServico.HasValue)
        {
            query = query.Where(a => a.NotaServico == notaServico.Value);
        }

        if (notaBarbeiro.HasValue)
        {
            query = query.Where(a => a.NotaBarbeiro == notaBarbeiro.Value);
        }

        if (!string.IsNullOrEmpty(observacao))
        {
            query = query.Where(a => a.Observacao.Contains(observacao));
        }

        return await query.ToListAsync();
    }


}
