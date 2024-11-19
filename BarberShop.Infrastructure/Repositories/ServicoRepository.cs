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
    public class ServicoRepository : IServicoRepository
    {
        private readonly BarbeariaContext _context;

        public ServicoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public async Task<Servico> AddAsync(Servico entity)
        {
            try
            {
                await _context.Servicos.AddAsync(entity);
                return entity; // Não salva aqui, pois será feito no SaveChangesAsync
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao adicionar o serviço", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var servico = await _context.Servicos.FindAsync(id);
                if (servico != null)
                {
                    _context.Servicos.Remove(servico);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir o serviço com Id {id}", ex);
            }
        }

        public async Task<IEnumerable<Servico>> GetAllAsync()
        {
            try
            {
                return await _context.Servicos.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter a lista de serviços", ex);
            }
        }

        public async Task<Servico> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Servicos.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter o serviço com Id {id}", ex);
            }
        }

        public async Task UpdateAsync(Servico entity)
        {
            try
            {
                _context.Servicos.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao atualizar o serviço", ex);
            }
        }

        public async Task<IEnumerable<Servico>> ObterServicosPorIdsAsync(IEnumerable<int> servicoIds)
        {
            try
            {
                return await _context.Servicos.Where(s => servicoIds.Contains(s.ServicoId)).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter os serviços por IDs", ex);
            }
        }

        public async Task<IEnumerable<Servico>> ObterServicosPorBarbeariaIdAsync(int barbeariaId)
        {
            return await _context.Servicos
                .Where(s => s.BarbeariaId == barbeariaId)
                .ToListAsync();
        }

        // Implementação do SaveChangesAsync
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> ObterNomesServicosAsync(IEnumerable<int> servicoIds)
        {
            try
            {
                return await _context.Servicos
                    .Where(s => servicoIds.Contains(s.ServicoId))
                    .Select(s => s.Nome)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter os nomes dos serviços por IDs", ex);
            }
        }
    }
}
