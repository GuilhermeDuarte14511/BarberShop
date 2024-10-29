namespace BarberShop.Application.Services
{
    public interface IEmailService
    {
        // Método para enviar email de confirmação de agendamento para o cliente
        Task EnviarEmailAgendamentoAsync(
            string destinatarioEmail,
            string destinatarioNome,
            string assunto,
            string conteudo,
            string barbeiroNome,
            DateTime dataHoraInicio,
            DateTime dataHoraFim,
            decimal total,
            string googleCalendarLink = null);

        // Método para enviar email de notificação de agendamento para o barbeiro
        Task EnviarEmailNotificacaoBarbeiroAsync(
            string barbeiroEmail,
            string barbeiroNome,
            string clienteNome,
            List<string> servicos,
            DateTime dataHoraInicio,
            DateTime dataHoraFim,
            decimal total);

        // Método para enviar email de código de verificação
        Task EnviarEmailCodigoVerificacaoAsync(
            string destinatarioEmail,
            string destinatarioNome,
            string codigoVerificacao);

        // Método para gerar link do Google Calendar
        string GerarLinkGoogleCalendar(
            string titulo,
            DateTime dataInicio,
            DateTime dataFim,
            string descricao,
            string local);
    }
}
