using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class Barbeiro
    {
        public int BarbeiroId { get; set; }  // Alterado para BarbeiroId para corresponder ao banco de dados
        public string Nome { get; set; }
        public string Email { get; set; }    // Adicionado conforme o banco de dados
        public string Telefone { get; set; } // Adicionado conforme o banco de dados
    }
}
