    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class CriarUsuarioDTO
    {
        public int? UsuarioId { get; set; } // Opcional para edição
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Role { get; set; } // "Admin" ou "Barbeiro"
        public int? Status { get; set; } // Ativo ou Inativo
        public int? BarbeariaId { get; set; } // Vinculação com a barbearia
        public string? TipoUsuario { get; set; } // "Admin" ou "Barbeiro"
    }
}
