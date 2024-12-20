﻿using BarberShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infrastructure.Data
{
    public class BarbeariaContext : DbContext
    {
        public BarbeariaContext(DbContextOptions<BarbeariaContext> options) : base(options)
        {
            Console.WriteLine("DbContext criado.");
        }

        public override void Dispose()
        {
            Console.WriteLine("DbContext descartado.");
            base.Dispose();
        }

        // DbSets para as entidades existentes
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Barbeiro> Barbeiros { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<AgendamentoServico> AgendamentoServicos { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RelatorioPersonalizado> RelatoriosPersonalizados { get; set; }
        public DbSet<GraficoPosicao> GraficoPosicao { get; set; }
        public DbSet<Avaliacao> Avaliacao { get; set; }
        public DbSet<Barbearia> Barbearias { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }

        // DbSets para as novas entidades
        public DbSet<PlanoAssinaturaSistema> PlanoAssinaturaSistema { get; set; }
        public DbSet<PlanoAssinaturaBarbearia> PlanoAssinaturaBarbearias { get; set; }
        public DbSet<PlanoBeneficio> PlanoBeneficios { get; set; }
        public DbSet<PagamentoAssinatura> PagamentosAssinaturasSite { get; set; }
        public DbSet<FeriadoNacional> FeriadosNacionais { get; set; }
        public DbSet<FeriadoBarbearia> FeriadosBarbearias { get; set; }
        public DbSet<IndisponibilidadeBarbeiro> IndisponibilidadesBarbeiros { get; set; }
        public DbSet<BarbeiroServico> BarbeiroServicos { get; set; } // Adicionado
        public DbSet<PushSubscription> PushSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela BarbeiroServico (Relacionamento N para N)
            modelBuilder.Entity<BarbeiroServico>()
                .HasKey(bs => new { bs.BarbeiroId, bs.ServicoId }); // Chave composta

            modelBuilder.Entity<BarbeiroServico>()
                .HasOne(bs => bs.Barbeiro)
                .WithMany(b => b.BarbeiroServicos)
                .HasForeignKey(bs => bs.BarbeiroId);

            modelBuilder.Entity<BarbeiroServico>()
                .HasOne(bs => bs.Servico)
                .WithMany(s => s.BarbeiroServicos)
                .HasForeignKey(bs => bs.ServicoId);


            // Configuração de chave primária para PlanoAssinaturaBarbearia
            modelBuilder.Entity<PlanoAssinaturaBarbearia>()
                .HasKey(p => p.PlanoBarbeariaId);

            // Configuração de chave primária para PlanoAssinaturaSistema
            modelBuilder.Entity<PlanoAssinaturaSistema>()
                .HasKey(p => p.PlanoId);

            // Configuração de chave primária para PagamentoAssinatura
            modelBuilder.Entity<PagamentoAssinatura>()
                .HasKey(p => p.AssinaturaId);

            // Configuração para PagamentoAssinatura
            modelBuilder.Entity<PagamentoAssinatura>()
                .Property(p => p.ValorPago)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PagamentoAssinatura>()
                .Property(p => p.DataPagamento)
                .IsRequired();

            // Configuração de FeriadosNacionais
            modelBuilder.Entity<FeriadoNacional>(entity =>
            {
                entity.HasKey(f => f.FeriadoId);
                entity.Property(f => f.Descricao).IsRequired().HasMaxLength(255);
                entity.Property(f => f.Data).IsRequired();
                entity.Property(f => f.Recorrente).IsRequired();
            });

            // Configuração de FeriadosBarbearias
            modelBuilder.Entity<FeriadoBarbearia>(entity =>
            {
                entity.HasKey(fb => fb.FeriadoBarbeariaId);
                entity.Property(fb => fb.Descricao).HasMaxLength(255);
                entity.Property(fb => fb.Data).IsRequired();
                entity.Property(fb => fb.Recorrente).IsRequired();

                // Relacionamento com Barbearia
                entity.HasOne(fb => fb.Barbearia)
                      .WithMany(b => b.FeriadosBarbearias)
                      .HasForeignKey(fb => fb.BarbeariaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração de IndisponibilidadesBarbeiros
            modelBuilder.Entity<IndisponibilidadeBarbeiro>(entity =>
            {
                entity.HasKey(i => i.IndisponibilidadeId);
                entity.Property(i => i.Motivo).HasMaxLength(255);

                // Relacionamento com Barbeiro
                entity.HasOne(i => i.Barbeiro)
                      .WithMany(b => b.Indisponibilidades)
                      .HasForeignKey(i => i.BarbeiroId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Outras configurações existentes
            modelBuilder.Entity<AgendamentoServico>()
                .HasKey(agendamentoServico => new { agendamentoServico.AgendamentoId, agendamentoServico.ServicoId });

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Pagamento)
                .WithOne(p => p.Agendamento)
                .HasForeignKey<Pagamento>(p => p.AgendamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pagamento>()
                .Property(p => p.ValorPago)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RelatorioPersonalizado>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.RelatoriosPersonalizados)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Barbearia>()
                .HasMany(b => b.Barbeiros)
                .WithOne(b => b.Barbearia)
                .HasForeignKey(b => b.BarbeariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Barbearia>()
                .HasMany(b => b.Servicos)
                .WithOne(s => s.Barbearia)
                .HasForeignKey(s => s.BarbeariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Barbearia>()
                .HasMany(b => b.Agendamentos)
                .WithOne(a => a.Barbearia)
                .HasForeignKey(a => a.BarbeariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Barbearia)
                .WithMany()
                .HasForeignKey(c => c.BarbeariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pagamento>()
                .HasOne(p => p.Barbearia)
                .WithMany()
                .HasForeignKey(p => p.BarbeariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Barbearia>()
                .HasIndex(b => b.UrlSlug)
                .IsUnique();

            modelBuilder.Entity<PlanoAssinaturaBarbearia>()
                .HasOne(p => p.Barbearia)
                .WithMany(b => b.PlanosAssinaturaBarbearias)
                .HasForeignKey(p => p.BarbeariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlanoBeneficio>()
                .HasOne(p => p.PlanoAssinaturaBarbearia)
                .WithMany()
                .HasForeignKey(p => p.PlanoBarbeariaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlanoBeneficio>()
                .HasOne(p => p.Servico)
                .WithMany()
                .HasForeignKey(p => p.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Agendamento)
                .WithMany(ag => ag.Avaliacoes)
                .HasForeignKey(a => a.AgendamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração da tabela Notificacoes
            modelBuilder.Entity<Notificacao>(entity =>
            {
                entity.HasKey(n => n.NotificacaoId);
                entity.Property(n => n.Mensagem).IsRequired().HasMaxLength(500);
                entity.Property(n => n.Link).HasMaxLength(255);
                entity.Property(n => n.Lida).HasDefaultValue(false);
                entity.Property(n => n.DataHora).IsRequired();

                // Relacionamento com Usuario
                entity.HasOne(n => n.Usuario)
                      .WithMany()
                      .HasForeignKey(n => n.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento com Barbearia
                entity.HasOne(n => n.Barbearia)
                      .WithMany()
                      .HasForeignKey(n => n.BarbeariaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
