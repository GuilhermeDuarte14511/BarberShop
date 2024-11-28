using BarberShop.Domain.Entities;

public class Notificacao
{
    public int NotificacaoId { get; set; }
    public int UsuarioId { get; set; }
    public int BarbeariaId { get; set; }
    public int? AgendamentoId { get; set; } 
    public string Mensagem { get; set; }
    public string Link { get; set; }
    public bool Lida { get; set; }
    public DateTime DataHora { get; set; }

    public virtual Usuario? Usuario { get; set; }
    public virtual Barbearia? Barbearia { get; set; }
    public virtual Agendamento? Agendamento { get; set; }

}
