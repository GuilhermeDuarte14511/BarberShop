using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class AtualizarBarbeiroUsuarioDto
    {
        public int UsuarioId { get; set; }
        public int BarbeiroId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
    }

}
