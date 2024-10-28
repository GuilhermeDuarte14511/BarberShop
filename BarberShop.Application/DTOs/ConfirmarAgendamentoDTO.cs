namespace BarberShop.Application.DTOs
{
    public class ConfirmarAgendamentoDTO
    {
        public int BarbeiroId { get; set; }
        public DateTime DataHora { get; set; }
        public List<int> ServicoIds { get; set; }
    }
}
