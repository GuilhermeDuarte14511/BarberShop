using BarberShop.Domain.Entities;
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
        public DbSet<RelatorioPersonalizado> RelatoriosPersonalizados { get; set; }
        public DbSet<GraficoPosicao> GraficoPosicao { get; set; }
        public DbSet<Avaliacao> Avaliacao { get; set; }
        public DbSet<Barbearia> Barbearias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração de chave composta para AgendamentoServico
            modelBuilder.Entity<AgendamentoServico>()
                .HasKey(agendamentoServico => new { agendamentoServico.AgendamentoId, agendamentoServico.ServicoId });

            // Configuração do relacionamento um-para-um entre Agendamento e Pagamento
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Pagamento)
                .WithOne(p => p.Agendamento)
                .HasForeignKey<Pagamento>(p => p.AgendamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração para coluna ValorPago em Pagamento
            modelBuilder.Entity<Pagamento>()
                .Property(p => p.ValorPago)
                .HasColumnType("decimal(18,2)");

            // Configuração para Usuario
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
                .WithMany(u => u.RelatoriosPersonalizados)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração para relacionamentos de Barbearia
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

            // Configuração para a propriedade BarbeariaId em Cliente e Pagamento
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

            // Configuração para a propriedade UrlSlug ser única em Barbearia
            modelBuilder.Entity<Barbearia>()
                .HasIndex(b => b.UrlSlug)
                .IsUnique();
        }
    }
}
