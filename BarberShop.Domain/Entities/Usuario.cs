using System;
using System.Collections.Generic;

namespace BarberShop.Domain.Entities
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public string Telefone { get; set; }
        public string Role { get; set; } // Valores possíveis: "Admin" ou "Barbeiro"
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public int Status { get; set; } = 1; // 1 para ativo, 0 para inativo

        public string? CodigoValidacao { get; set; }
        public DateTime? CodigoValidacaoExpiracao { get; set; }

        public string? TokenRecuperacaoSenha { get; set; }
        public DateTime? TokenExpiracao { get; set; }

        public int BarbeariaId { get; set; }
        public int? BarbeiroId { get; set; }
        public Barbearia Barbearia { get; set; }

        public ICollection<RelatorioPersonalizado> RelatoriosPersonalizados { get; set; } = new List<RelatorioPersonalizado>();
    }

}
