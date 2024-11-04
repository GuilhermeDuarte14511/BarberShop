using System;

namespace BarberShop.Domain.Entities
{
    public class RelatorioPersonalizado
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } // Id do usuário, caso tenha login/sistema de usuários
        public string TipoRelatorio { get; set; } // Ex: 'pagamentosPorStatus'
        public int PeriodoDias { get; set; } // Ex: 30, 15, 7
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public string Configuracoes { get; set; } // Armazena configurações adicionais, caso precise (JSON, XML etc.)

        // Propriedade de navegação para o usuário
        public Usuario Usuario { get; set; }
    }
}
