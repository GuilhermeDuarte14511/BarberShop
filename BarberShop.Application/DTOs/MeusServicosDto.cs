using System.Collections.Generic;

namespace BarberShop.Application.Dtos
{
    public class MeusServicosDto
    {
        public int BarbeiroId { get; set; }
        public int BarbeariaId { get; set; }
        public IEnumerable<ServicoDto> ServicosVinculados { get; set; }
        public IEnumerable<ServicoDto> ServicosNaoVinculados { get; set; }
    }

    public class ServicoDto
    {
        public int ServicoId { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Duracao { get; set; } // Em minutos
    }
}
