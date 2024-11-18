using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class IndisponibilidadeBarbeiro
    {
        public int IndisponibilidadeId { get; set; } // Identificador único
        public int BarbeiroId { get; set; } // Relacionamento com o barbeiro
        public DateTime DataInicio { get; set; } // Data de início da indisponibilidade
        public DateTime DataFim { get; set; } // Data de fim da indisponibilidade
        public string Motivo { get; set; } // Motivo opcional da indisponibilidade

        // Propriedade de navegação
        public Barbeiro Barbeiro { get; set; }

    }
}
