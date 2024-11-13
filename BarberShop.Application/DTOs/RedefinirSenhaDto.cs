using System;

namespace BarberShop.Application.DTOs
{
    public class RedefinirSenhaDto
    {
        public int ClienteId { get; set; } 
        public string Token { get; set; }
        public string NovaSenha { get; set; }
    }
}
