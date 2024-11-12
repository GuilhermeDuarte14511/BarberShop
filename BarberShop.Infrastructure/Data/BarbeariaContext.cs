using BarberShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace BarberShop.Infrastructure.Data
{
    public class BarbeariaContext : DbContext
    {
        public BarbeariaContext(DbContextOptions<BarbeariaContext> options)
            : base(options)
        {
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

        // DbSets para as novas entidades
        public DbSet<PlanoAssinaturaSistema> PlanoAssinaturaSistema { get; set; }
        public DbSet<PlanoAssinaturaBarbearia> PlanoAssinaturaBarbearias { get; set; }
        public DbSet<PlanoBeneficio> PlanoBeneficios { get; set; }
        public DbSet<PagamentoAssinatura> PagamentosAssinaturasSite { get; set; } // Novo DbSet para PagamentoAssinatura

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Outras configurações já existentes
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
        }
    }
}
