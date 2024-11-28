namespace BarberShop.Application.DTOs
{
    public class NotificacaoDTO
    {
        public int NotificacaoId { get; set; }
        public int? UsuarioId { get; set; }
        public int BarbeariaId { get; set; }
        public int? AgendamentoId { get; set; } // Adicionado
        public string Mensagem { get; set; } 
        public string Link { get; set; } 
        public bool Lida { get; set; } 
        public DateTime DataHora { get; set; }

        public string DataHoraFormatada => DataHora.ToString("dd/MM/yyyy HH:mm"); // Data formatada
    }
}
