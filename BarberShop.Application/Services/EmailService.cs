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

        public async Task EnviarEmailAgendamentoAsync(string destinatarioEmail, string destinatarioNome, string assunto, string conteudo, string barbeiroNome, DateTime dataHoraInicio,
            DateTime dataHoraFim, decimal total, string formaPagamento, string googleCalendarLink = null)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("projetobarberia14511@outlook.com", "Barbearia CG DREAMS");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);

                // Estilizando o HTML do e-mail com as cores do projeto
                string htmlContent = $@"
                    <html>
                    <head>
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
                                color: #000000; /* Texto em preto */
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
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Confirmação de Agendamento</h1>
                            <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                            <p>Aqui está o resumo do seu agendamento:</p>
                
                            <div class='details'>
                                <p><strong>Barbeiro:</strong> {barbeiroNome}</p>
                                <p><strong>Data e Hora de Início:</strong> {dataHoraInicio:dd/MM/yyyy - HH:mm}</p>
                                <p><strong>Data e Hora de Fim:</strong> {dataHoraFim:dd/MM/yyyy - HH:mm}</p>
                                <p><strong>Forma de Pagamento:</strong> {formaPagamento}</p>
                                <p><strong>Valor Total:</strong> R$ {total:F2}</p>
                            </div>";

                if (!string.IsNullOrEmpty(googleCalendarLink))
                {
                    htmlContent += $@"
                            <p style='text-align: center;'>
                                <a href='{googleCalendarLink}' class='btn' style='color: #000000;'>Adicionar ao Google Calendar</a>
                            </p>";
                }

                htmlContent += @"
                            <p>Obrigado por escolher a Barbearia CG DREAMS!</p>
                            <div class='footer'>
                                <p>&copy; " + DateTime.Now.Year + @" Barbearia CG DREAMS. Todos os direitos reservados.</p>
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

        public async Task EnviarEmailNotificacaoBarbeiroAsync(string barbeiroEmail, string barbeiroNome, string clienteNome, List<string> servicos, DateTime dataHoraInicio, DateTime dataHoraFim, decimal total, string formaPagamento)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("projetobarberia14511@outlook.com", "Barbearia CG DREAMS");
                var to = new EmailAddress(barbeiroEmail, barbeiroNome);

                string htmlContent = $@"
                        <html>
                        <head>
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
                                .footer {{
                                    text-align: center;
                                    margin-top: 20px;
                                    font-size: 12px;
                                    color: #99aab5;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>Novo Agendamento Recebido</h1>
                                <p>Olá, <strong>{barbeiroNome}</strong>,</p>
                                <p>Um novo agendamento foi realizado. Confira os detalhes:</p>
        
                                <div class='details'>
                                    <p><strong>Cliente:</strong> {clienteNome}</p>
                                    <p><strong>Data e Hora de Início:</strong> {dataHoraInicio:dd/MM/yyyy - HH:mm}</p>
                                    <p><strong>Data e Hora de Fim:</strong> {dataHoraFim:dd/MM/yyyy - HH:mm}</p>
                                    <p><strong>Forma de Pagamento:</strong> {formaPagamento}</p>
                                    <p><strong>Valor Total:</strong> R$ {total:F2}</p>
                                    <p><strong>Serviços Solicitados:</strong></p>
                                    <ul>";

                foreach (var servico in servicos)
                {
                    htmlContent += $"<li>{servico}</li>";
                }

                htmlContent += @"
                                    </ul>
                                </div>
                                <p>Por favor, prepare-se para o atendimento.</p>
                                <div class='footer'>
                                    <p>&copy; " + DateTime.Now.Year + @" Barbearia CG DREAMS. Todos os direitos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                var assunto = "Novo Agendamento Confirmado";
                var msg = MailHelper.CreateSingleEmail(from, to, assunto, string.Empty, htmlContent);

                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                Console.WriteLine($"E-mail de notificação enviado com sucesso para: {barbeiroEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de notificação para {barbeiroEmail}: {ex.Message}");
                throw;
            }
        }

        // Novo método para enviar email de código de verificação
        public async Task EnviarEmailCodigoVerificacaoAsync(string destinatarioEmail, string destinatarioNome, string codigoVerificacao)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("projetobarberia14511@outlook.com", "Barbearia CG DREAMS");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Seu Código de Verificação";
                var conteudo = $"Olá, {destinatarioNome}!\n\nSeu código de verificação é: {codigoVerificacao}\n\nEste código expira em 5 minutos.";

                string htmlContent = $@"
                <html>
                <head>
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
                        .code {{
                            background-color: #99aab5;
                            color: #23272a;
                            font-size: 24px;
                            font-weight: bold;
                            text-align: center;
                            padding: 15px;
                            border-radius: 8px;
                            margin: 20px 0;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #99aab5;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Código de Verificação</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                        <p>Seu código de verificação é:</p>
                        <div class='code'>{codigoVerificacao}</div>
                        <p>Este código expira em <strong>5 minutos</strong>.</p>
                        <p>Se você não solicitou este código, por favor ignore este email.</p>
                        <div class='footer'>
                            <p>&copy; " + DateTime.Now.Year + @" Barbearia CG DREAMS. Todos os direitos reservados.</p>
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

                Console.WriteLine($"E-mail de código de verificação enviado com sucesso para: {destinatarioEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail para {destinatarioEmail}: {ex.Message}");
                throw;
            }
        }

        // Método para gerar o link do Google Calendar
        public string GerarLinkGoogleCalendar(string titulo, DateTime dataInicio, DateTime dataFim, string descricao, string local)
        {
            dataInicio = dataInicio.ToUniversalTime();
            dataFim = dataFim.ToUniversalTime();

            string dataInicioFormatada = dataInicio.ToString("yyyyMMddTHHmmssZ");
            string dataFimFormatada = dataFim.ToString("yyyyMMddTHHmmssZ");

            return $"https://www.google.com/calendar/render?action=TEMPLATE&text={Uri.EscapeDataString(titulo)}&dates={dataInicioFormatada}/{dataFimFormatada}&details={Uri.EscapeDataString(descricao)}&location={Uri.EscapeDataString(local)}";
        }
    }
}
