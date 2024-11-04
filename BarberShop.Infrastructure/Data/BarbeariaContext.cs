﻿using BarberShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infrastructure.Data
{
    public class BarbeariaContext : DbContext
    {
        public BarbeariaContext(DbContextOptions<BarbeariaContext> options)
            : base(options)
        {
        }

        // DbSets para as entidades
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Barbeiro> Barbeiros { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<AgendamentoServico> AgendamentoServicos { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RelatorioPersonalizado> RelatoriosPersonalizados { get; set; } // Novo DbSet para RelatorioPersonalizado

        public DbSet<GraficoPosicao> GraficoPosicao { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definir chave composta para a entidade AgendamentoServico
            modelBuilder.Entity<AgendamentoServico>()
                .HasKey(agendamentoServico => new { agendamentoServico.AgendamentoId, agendamentoServico.ServicoId });

            // Configura o relacionamento um-para-um entre Agendamento e Pagamento
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Pagamento)
                .WithOne(p => p.Agendamento)
                .HasForeignKey<Pagamento>(p => p.AgendamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração do tipo de coluna para ValorPago em Pagamento
            modelBuilder.Entity<Pagamento>()
                .Property(p => p.ValorPago)
                .HasColumnType("decimal(18,2)");

            // Configuração para a entidade Usuario
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configuração para RelatorioPersonalizado
            modelBuilder.Entity<RelatorioPersonalizado>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.RelatoriosPersonalizados) // Assumindo que a propriedade de navegação foi adicionada em Usuario
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
