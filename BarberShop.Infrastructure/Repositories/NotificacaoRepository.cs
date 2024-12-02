using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BarberShop.Infrastructure.Repositories
{
    public class NotificacaoRepository : INotificacaoRepository
    {
        private readonly BarbeariaContext _context;

        public NotificacaoRepository(BarbeariaContext context)
        {
            _context = context;
        }

        public void Criar(Notificacao notificacao)
        {
            _context.Notificacoes.Add(notificacao);
            _context.SaveChanges();
        }

        public IEnumerable<Notificacao> ObterPorUsuario(int usuarioId)
        {
            return _context.Notificacoes.Where(n => n.UsuarioId == usuarioId).ToList();
        }

        public IEnumerable<Usuario> ObterAdminsPorBarbearia(int barbeariaId)
        {
            return _context.Usuarios
                .Where(u => u.BarbeariaId == barbeariaId && u.Role == "Admin" && u.Status == 1)
                .ToList();
        }

        public Usuario ObterBarbeiroPorId(int barbeiroId)
        {
            return _context.Usuarios
                .FirstOrDefault(u => u.BarbeiroId == barbeiroId && u.Role == "Barbeiro" && u.Status == 1);
        }

        public IEnumerable<Agendamento> ObterAgendamentosDoDia(DateTime data)
        {
            var inicioDoDia = data.Date;
            var fimDoDia = data.Date.AddDays(1).AddTicks(-1);

            return _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .Where(a => a.DataHora >= inicioDoDia && a.DataHora <= fimDoDia
                            && (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado))
                .ToList();
        }



        public Notificacao ObterUltimaNotificacao(int usuarioId, int agendamentoId, DateTime referencia)
        {
            return _context.Notificacoes
                .Where(n => n.UsuarioId == usuarioId && n.AgendamentoId == agendamentoId && n.DataHora >= referencia)
                .OrderByDescending(n => n.DataHora)
                .FirstOrDefault();
        }

        public void SalvarAlteracoes()
        {
            _context.SaveChanges();
        }

        public IEnumerable<Usuario> ObterBarbeirosPorBarbearia(int barbeariaId)
        {
            return _context.Usuarios
                .Where(u => u.BarbeariaId == barbeariaId && u.Role == "Barbeiro" && u.Status == 1)
                .ToList();
        }

        public IEnumerable<Notificacao> ObterNotificacoes(int usuarioId, int agendamentoId)
        {
            return _context.Notificacoes
                .Where(n => n.UsuarioId == usuarioId && n.AgendamentoId == agendamentoId)
                .ToList();
        }

        public Barbearia ObterBarbeariaPorId(int barbeariaId)
        {
            return _context.Barbearias.FirstOrDefault(b => b.BarbeariaId == barbeariaId);
        }

        public Agendamento ObterAgendamentoPorId(int agendamentoId)
        {
            return _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Barbeiro)
                .FirstOrDefault(a => a.AgendamentoId == agendamentoId);
        }


    }
}
