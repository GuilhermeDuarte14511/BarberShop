using System;

namespace BarberShop.Domain.Entities
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string? Senha { get; set; }
        public string? CodigoValidacao { get; set; }
        public DateTime? CodigoValidacaoExpiracao { get; set; }
        public string Role { get; set; }

        public string? TokenRecuperacaoSenha { get; set; }
        public DateTime? TokenExpiracao { get; set; }
        public int BarbeariaId { get; set; }
        public Barbearia Barbearia { get; set; }
    }
}
