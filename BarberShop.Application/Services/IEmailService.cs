namespace BarberShop.Application.Services
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatarioEmail, string destinatarioNome, string assunto, string conteudo, string barbeiroNome, DateTime dataHoraInicio, DateTime dataHoraFim, decimal total, string googleCalendarLink = null);
        string GerarLinkGoogleCalendar(string titulo, DateTime dataInicio, DateTime dataFim, string descricao, string local);
    }
}
