using SendGrid;
using SendGrid.Helpers.Mail;

namespace BarberShop.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _sendGridApiKey;

        public EmailService(string sendGridApiKey)
        {
            _sendGridApiKey = sendGridApiKey;
        }

        public async Task EnviarEmailAsync(string destinatarioEmail, string destinatarioNome, string assunto, string conteudo, string barbeiroNome, DateTime dataHoraInicio, DateTime dataHoraFim, decimal total, string googleCalendarLink = null)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("guilhermeduarte14511@gmail.com", "Barbearia Neris");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);

                // Estilizando o HTML do e-mail com as cores do projeto
                string htmlContent = $@"
                <html>
                <head>
                    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
                    <style>
                        body {{
                            font-family: 'Arial', sans-serif;
                            background-color: #2c2f33;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            background-color: #23272a;
                            color: #ffffff;
                            max-width: 600px;
                            margin: 20px auto;
                            border-radius: 10px;
                            padding: 20px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        h1 {{
                            font-size: 24px;
                            color: #e74c3c;
                            text-align: center;
                            border-bottom: 2px solid #e74c3c;
                            padding-bottom: 10px;
                            margin-bottom: 20px;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.6;
                            color: #ffffff;
                        }}
                        .details {{
                            background-color: #99aab5;
                            padding: 15px;
                            border-radius: 8px;
                            margin-bottom: 20px;
                            color: #23272a;
                        }}
                        .btn {{
                            background-color: #e74c3c;
                            color: #ffffff;
                            padding: 10px 15px;
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold;
                            display: inline-block;
                        }}
                        .btn:hover {{
                            background-color: #c0392b;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #99aab5;
                        }}
                        .gif-container {{
                            text-align: center;
                            margin: 20px 0;
                        }}
                        .gif-container img {{
                            max-width: 100%;
                            border-radius: 10px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Confirmação de Agendamento</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong></p>
                        <p>Aqui está o resumo do seu agendamento:</p>
                        
                        <div class='details'>
                            <p><strong>Barbeiro:</strong> {barbeiroNome}</p>
                            <p><strong>Data e Hora de Início:</strong> {dataHoraInicio:dd/MM/yyyy - HH:mm}</p>
                            <p><strong>Data e Hora de Fim:</strong> {dataHoraFim:dd/MM/yyyy - HH:mm}</p>
                            <p><strong>Valor Total:</strong> R$ {total}</p>
                        </div>";

                if (!string.IsNullOrEmpty(googleCalendarLink))
                {
                    htmlContent += $@"
                        <p class='text-center'>
                            <a href='{googleCalendarLink}' class='btn'>Adicionar ao Google Calendar</a>
                        </p>";
                }

                htmlContent += @"
                        <div class='gif-container'>
                            <img src='https://example.com/path/to/barber-gif.gif' alt='Barbearia' />
                        </div>
                        <p>Obrigado por escolher a Barbearia Neris!</p>
                        <div class='footer'>
                            <p>&copy; 2024 Barbearia Neris. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);

                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                Console.WriteLine($"E-mail enviado com sucesso para: {destinatarioEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail para {destinatarioEmail}: {ex.Message}");
                throw;
            }
        }

        public string GerarLinkGoogleCalendar(string titulo, DateTime dataInicio, DateTime dataFim, string descricao, string local)
        {
            // Ajustando o fuso horário para UTC (adicionando 3 horas)
            dataInicio = dataInicio.AddHours(3);
            dataFim = dataFim.AddHours(3);

            string dataInicioFormatada = dataInicio.ToString("yyyyMMddTHHmmssZ");
            string dataFimFormatada = dataFim.ToString("yyyyMMddTHHmmssZ");

            return $"https://www.google.com/calendar/render?action=TEMPLATE&text={Uri.EscapeDataString(titulo)}&dates={dataInicioFormatada}/{dataFimFormatada}&details={Uri.EscapeDataString(descricao)}&location={Uri.EscapeDataString(local)}";
        }
    }
}
