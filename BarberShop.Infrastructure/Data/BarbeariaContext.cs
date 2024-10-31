using BarberShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infrastructure.Data
{
    public class BarbeariaContext : DbContext
    {
        // Construtor necessário para o Entity Framework
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
        public DbSet<Log> Logs { get; set; } // Adicione essa linha para a tabela de logs


        // Configuração adicional
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definir chave composta para a entidade AgendamentoServico
            modelBuilder.Entity<AgendamentoServico>()
                .HasKey(agendamentoServico => new { agendamentoServico.AgendamentoId, agendamentoServico.ServicoId });

            // Configuração adicional pode ser feita aqui
            // Exemplo: Definição de relacionamento, configurações de propriedade, etc.
        }
    }
}
