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
    public class PagamentoRepository : Repository<Pagamento>, IPagamentoRepository
    {
        private readonly BarbeariaContext _context;

        public PagamentoRepository(BarbeariaContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Pagamento>> GetAllAsync()
        {
            try
            {
                return await _context.Pagamentos
                    .Include(p => p.Agendamento)
                    .ThenInclude(a => a.Cliente)
                    .OrderByDescending(p => p.PagamentoId)
                    .Select(p => new Pagamento
                    {
                        PagamentoId = p.PagamentoId,
                        AgendamentoId = p.AgendamentoId,
                        ClienteId = p.Agendamento.Cliente.ClienteId,
                        Cliente = new Cliente
                        {
                            ClienteId = p.Agendamento.Cliente.ClienteId,
                            Nome = p.Agendamento.Cliente.Nome
                        },
                        ValorPago = p.ValorPago,
                        StatusPagamento = p.StatusPagamento,
                        PaymentId = p.PaymentId ?? string.Empty,
                        DataPagamento = p.DataPagamento
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao recuperar todos os pagamentos.", ex);
            }
        }

        public async Task<IEnumerable<Pagamento>> GetAllPagamentosByBarbeariaIdAsync(int barbeariaId)
        {
            try
            {
                return await _context.Pagamentos
                    .Where(p => p.Agendamento.BarbeariaId == barbeariaId)
                    .Include(p => p.Agendamento)
                    .ThenInclude(a => a.Cliente)
                    .OrderByDescending(p => p.PagamentoId)
                    .Select(p => new Pagamento
                    {
                        PagamentoId = p.PagamentoId,
                        AgendamentoId = p.AgendamentoId,
                        ClienteId = p.Agendamento.Cliente.ClienteId,
                        Cliente = new Cliente
                        {
                            ClienteId = p.Agendamento.Cliente.ClienteId,
                            Nome = p.Agendamento.Cliente.Nome
                        },
                        ValorPago = p.ValorPago,
                        StatusPagamento = p.StatusPagamento,
                        PaymentId = p.PaymentId ?? string.Empty,
                        DataPagamento = p.DataPagamento
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao recuperar pagamentos para a barbearia com ID {barbeariaId}.", ex);
            }
        }

        public override async Task<Pagamento> GetByIdAsync(int id)
        {
            try
            {
                var pagamento = await _context.Pagamentos
                    .Include(p => p.Agendamento)
                    .ThenInclude(a => a.Cliente)
                    .FirstOrDefaultAsync(p => p.PagamentoId == id);

                if (pagamento != null)
                {
                    if (pagamento.Agendamento == null)
                        pagamento.Agendamento = new Agendamento();

                    if (pagamento.Agendamento.Cliente == null)
                        pagamento.Agendamento.Cliente = new Cliente();
                }

                return pagamento;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao recuperar o pagamento com ID {id}.", ex);
            }
        }

        public override async Task<Pagamento> AddAsync(Pagamento entity)
        {
            try
            {
                await _context.Pagamentos.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao adicionar novo pagamento.", ex);
            }
        }

        public override async Task UpdateAsync(Pagamento entity)
        {
            try
            {
                _context.Pagamentos.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar o pagamento com ID {entity.PagamentoId}.", ex);
            }
        }

        public override async Task DeleteAsync(int id)
        {
            try
            {
                var pagamento = await GetByIdAsync(id);
                if (pagamento != null)
                {
                    _context.Pagamentos.Remove(pagamento);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir o pagamento com ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Pagamento>> GetPagamentosPorAgendamentoIdAsync(int agendamentoId)
        {
            try
            {
                return await _context.Pagamentos
                    .Where(p => p.AgendamentoId == agendamentoId)
                    .Include(p => p.Agendamento)
                    .ThenInclude(a => a.Cliente)
                    .Select(p => new Pagamento
                    {
                        PagamentoId = p.PagamentoId,
                        AgendamentoId = p.AgendamentoId,
                        ClienteId = p.Agendamento.Cliente.ClienteId,
                        ValorPago = p.ValorPago ?? 0m,
                        StatusPagamento = p.StatusPagamento,
                        PaymentId = p.PaymentId ?? string.Empty,
                        DataPagamento = p.DataPagamento,
                        BarbeariaId = p.BarbeariaId
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao recuperar pagamentos para o agendamento com ID {agendamentoId}.", ex);
            }
        }

    }
}
