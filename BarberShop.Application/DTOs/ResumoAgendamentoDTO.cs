using BarberShop.Application.DTOs;

public class ResumoAgendamentoDTO
{
    public int BarbeiroId { get; set; }
    public int BarbeariaId { get; set; }
    public string NomeBarbeiro { get; set; }
    public DateTime DataHora { get; set; }
    public List<ServicoDTO> ServicosSelecionados { get; set; }
    public decimal PrecoTotal { get; set; }
    public string FormaPagamento { get; set; }  // Nova propriedade para armazenar a forma de pagamento
    public string BarbeariaUrl { get; set; }  // Nova propriedade para armazenar a forma de pagamento
}
