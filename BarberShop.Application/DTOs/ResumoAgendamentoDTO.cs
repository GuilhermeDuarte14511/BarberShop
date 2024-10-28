namespace BarberShop.Application.DTOs
{
    public class ResumoAgendamentoDTO
    {
        public int BarbeiroId { get; set; }  // Adicionando o ID do barbeiro
        public string NomeBarbeiro { get; set; }
        public DateTime DataHora { get; set; }
        public List<ServicoDTO> ServicosSelecionados { get; set; }
        public decimal PrecoTotal { get; set; }
    }
}
