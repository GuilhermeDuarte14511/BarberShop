using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class CadastroRequestDTO
    {
        // Dados da Barbearia
        public string NomeBarbearia { get; set; }
        public string EmailBarbearia { get; set; }  // Email da Barbearia
        public string Endereco { get; set; }
        public string? Numero { get; set; }  // Número do endereço
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Cep { get; set; }
        public string TelefoneBarbearia { get; set; }

        // Dados do Administrador
        public string NomeAdmin { get; set; }
        public string Email { get; set; }
        public string TelefoneAdmin { get; set; }
        public string Senha { get; set; }
    }

}
