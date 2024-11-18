using System;
using System.Collections.Generic;

namespace BarberShop.Domain.Entities
{
    public class Barbeiro
    {
        public int BarbeiroId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public byte[]? Foto { get; set; } // Propriedade para armazenar a foto do barbeiro
        public int BarbeariaId { get; set; }
        public Barbearia Barbearia { get; set; }

        // Relacionamento com Indisponibilidades
        public ICollection<IndisponibilidadeBarbeiro> Indisponibilidades { get; set; }

        public Barbeiro()
        {
            Indisponibilidades = new List<IndisponibilidadeBarbeiro>();
        }
    }
}
