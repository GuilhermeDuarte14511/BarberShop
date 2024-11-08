using System;

namespace BarberShop.Domain.Entities
{
    public class Barbeiro
    {
        public int BarbeiroId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }

        public int BarbeariaId { get; set; }
        public Barbearia Barbearia { get; set; }
    }
}
