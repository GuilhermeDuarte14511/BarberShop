using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BarberShop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _sendGridApiKey;
        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;

        public EmailService(string sendGridApiKey, ILogService logService, IConfiguration configuration)
        {
            _sendGridApiKey = sendGridApiKey;
            _logService = logService;
            _configuration = configuration;
        }

        public async Task EnviarEmailAgendamentoAsync(string destinatarioEmail, string destinatarioNome, string assunto, string conteudo, string barbeiroNome, DateTime dataHoraInicio,
            DateTime dataHoraFim, decimal total, string formaPagamento, string nomeBarbearia, string googleCalendarLink = null)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de agendamento para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);

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
                                color: #000000;
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

                htmlContent += $@"
                            <p>Obrigado por escolher a {nomeBarbearia}!</p>
                            <div class='footer'>
                                <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailNotificacaoBarbeiroAsync(string barbeiroEmail, string barbeiroNome, string clienteNome, List<string> servicos, DateTime dataHoraInicio, DateTime dataHoraFim, decimal total, string formaPagamento, string nomeBarbearia)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de notificação para {barbeiroEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia);
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

                htmlContent += $@"
                                    </ul>
                                </div>
                                <p>Por favor, prepare-se para o atendimento.</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                var assunto = "Novo Agendamento Confirmado";
                var msg = MailHelper.CreateSingleEmail(from, to, assunto, string.Empty, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de notificação enviado com sucesso para: {barbeiroEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de notificação para {barbeiroEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailCodigoVerificacaoAsync(string destinatarioEmail, string destinatarioNome, string codigoVerificacao, string nomeBarbearia)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de verificação para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia);
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
                            <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de verificação enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de verificação para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public string GerarLinkGoogleCalendar(string titulo, DateTime dataInicio, DateTime dataFim, string descricao, string local)
        {
            dataInicio = dataInicio.ToUniversalTime();
            dataFim = dataFim.ToUniversalTime();

            string dataInicioFormatada = dataInicio.ToString("yyyyMMddTHHmmssZ");
            string dataFimFormatada = dataFim.ToString("yyyyMMddTHHmmssZ");

            return $"https://www.google.com/calendar/render?action=TEMPLATE&text={Uri.EscapeDataString(titulo)}&dates={dataInicioFormatada}/{dataFimFormatada}&details={Uri.EscapeDataString(descricao)}&location={Uri.EscapeDataString(local)}";
        }

        public async Task EnviarEmailFalhaCadastroAsync(string destinatarioEmail, string destinatarioNome, string nomeBarbearia)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de e-mail de falha de cadastro para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Falha no Cadastro - Assistência Necessária";
                var conteudo = $"Olá, {destinatarioNome}!\n\nOcorreu um problema ao concluir o seu cadastro, mas não se preocupe! Nossa equipe está pronta para ajudar você.";

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
                                .contact {{
                                    font-weight: bold;
                                    color: #99aab5;
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
                                <h1>Assistência no Cadastro</h1>
                                <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                                <p>Ocorreu um problema ao processar o seu cadastro. Nossa equipe está à disposição para resolver isso o mais rápido possível.</p>
                                <p>Por favor, entre em contato conosco pelos canais abaixo para que possamos ajudá-lo:</p>
                                <p class='contact'>WhatsApp: (XX) XXXXX-XXXX<br>
                                E-mail: suporte@cgdreams.com</p>
                                <p>Agradecemos pela sua paciência e estamos ansiosos para atendê-lo.</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de falha de cadastro, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail de falha de cadastro, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de falha de cadastro enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de falha de cadastro para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailRecuperacaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRecuperacao)
        {
            var assunto = "Redefinição de Senha";
            var conteudo = $"Olá, {destinatarioNome}!\n\nClique no link abaixo para redefinir sua senha:\n{linkRecuperacao}\n\nEste link expira em 1 hora.";

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
                        .link {{
                            display: block;
                            width: fit-content;
                            background-color: #99aab5;
                            color: #23272a;
                            font-size: 18px;
                            font-weight: bold;
                            text-align: center;
                            padding: 15px;
                            border-radius: 8px;
                            margin: 20px auto;
                            text-decoration: none;
                        }}
                        .link:hover {{
                            background-color: #7289da;
                            color: #ffffff;
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
                        <h1>Redefinição de Senha</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                        <p>Para redefinir sua senha, clique no link abaixo:</p>
                        <a href='{linkRecuperacao}' class='link'>Redefinir Senha</a>
                        <p>Este link expira em <strong>1 hora</strong>.</p>
                        <p>Se você não solicitou a redefinição de senha, por favor ignore este e-mail.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} BarberShop. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var from = new EmailAddress("barbershoperbrasil@outlook.com", "BarberShop");
            var to = new EmailAddress(destinatarioEmail, destinatarioNome);
            var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);

            var client = new SendGridClient(_sendGridApiKey);
            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de redefinição de senha, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
            }

            await _logService.SaveLogAsync("EmailService", $"E-mail de redefinição de senha enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
        }

        public async Task EnviaEmailAvaliacao(int agendamentoId, string destinatarioEmail, string destinatarioNome, string nomeBarbearia, string urlBase)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de avaliação para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Nos avalie - Sua opinião é muito importante!";

                // URL de avaliação
                var avaliacaoUrl = $"{urlBase}/Avaliacao/Index?agendamentoId={agendamentoId}";

                // Conteúdo do e-mail
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
                                .link {{
                                    display: block;
                                    width: fit-content;
                                    background-color: #e74c3c;
                                    color: #ffffff;
                                    font-size: 18px;
                                    font-weight: bold;
                                    text-align: center;
                                    padding: 15px;
                                    border-radius: 8px;
                                    margin: 20px auto;
                                    text-decoration: none;
                                }}
                                .link:hover {{
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
                                <h1>Ajude-nos a melhorar!</h1>
                                <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                                <p>Gostaríamos de saber como foi a sua experiência em nossa barbearia.</p>
                                <p>Clique no botão abaixo para avaliar o seu atendimento:</p>
                                <a href='{avaliacaoUrl}' class='link'>Avaliar Agora</a>
                                <p>Agradecemos o seu tempo e feedback!</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                // Criação do e-mail
                var msg = MailHelper.CreateSingleEmail(from, to, assunto, string.Empty, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de avaliação para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailBoasVindasAsync(string destinatarioEmail, string destinatarioNome, string senha, string tipoUsuario, string nomeBarbearia = null, string urlSlug = null)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de boas-vindas para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia ?? "BarberShop System");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Bem-vindo(a) ao Sistema BarberShop!";

                string saudacao = nomeBarbearia != null
                    ? $"Bem-vindo(a) ao sistema da barbearia <strong>{nomeBarbearia}</strong>!"
                    : "Bem-vindo(a) ao nosso sistema!";

                // Recupera o BaseUrl do appSettings
                string baseUrl = _configuration["AppSettings:BaseUrl"];

                // Constrói a URL de acesso
                string accessUrl = $"{baseUrl}/{urlSlug}/Admin";

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
                                            .button {{
                                                display: inline-block;
                                                padding: 10px 20px;
                                                font-size: 16px;
                                                color: #ffffff;
                                                background-color: #e74c3c;
                                                text-decoration: none;
                                                border-radius: 5px;
                                                margin-top: 20px;
                                            }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='container'>
                                            <h1>Bem-vindo(a), {destinatarioNome}!</h1>
                                            <p>{saudacao}</p>
                                            <p>Você foi registrado(a) como <strong>{tipoUsuario}</strong>.</p>
                                            <p>Aqui estão suas credenciais de acesso:</p>
                                            <div class='details'>
                                                <p><strong>Login:</strong> {destinatarioEmail}</p>
                                                <p><strong>Senha:</strong> {senha}</p>
                                            </div>
                                            <p>Por favor, altere sua senha após o primeiro acesso para garantir sua segurança.</p>
                                            <p>Para acessar o sistema, clique no botão abaixo:</p>
                                            <p style='text-align: center;'>
                                                <a href='{accessUrl}' class='buttonEmail'>Acesse por aqui</a>
                                            </p>
                                            <p>Se precisar de ajuda, entre em contato com o suporte.</p>
                                            <div class='footer'>
                                                <p>&copy; {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.</p>
                                            </div>
                                        </div>
                                    </body>
                                    </html>";

                var plainTextContent = $@"
                Bem-vindo(a), {destinatarioNome}!

                {saudacao}

                Você foi registrado(a) como {tipoUsuario}.

                Suas credenciais de acesso são:
                Login: {destinatarioEmail}
                Senha: {senha}

                Para acessar o sistema, visite: {accessUrl}

                Por favor, altere sua senha após o primeiro acesso.

                Se precisar de ajuda, entre em contato com o suporte.

                © {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de boas-vindas, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de boas-vindas enviado com sucesso para {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de boas-vindas para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }


        public async Task EnviarEmailCancelamentoAgendamentoAsync(string destinatarioEmail,string destinatarioNome,string nomeBarbearia,DateTime dataHora,string barbeiroNome, string baseUrl)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de cancelamento para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershoperbrasil@outlook.com", nomeBarbearia ?? "BarberShop System");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Agendamento Cancelado - Informações Importantes";

                // Gerar URL para reagendamento com base no esquema, domínio e nome da barbearia
                var urlReagendar = $"{baseUrl}/{nomeBarbearia.ToLower().Replace(" ", "")}";

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
                        .cta {{
                            text-align: center;
                            margin-top: 20px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 12px 25px;
                            font-size: 16px;
                            color: #ffffff;
                            background-color: #e74c3c;
                            text-decoration: none;
                            border-radius: 20px; /* Bordas arredondadas */
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
                            transition: background-color 0.3s ease, box-shadow 0.3s ease;
                        }}
                        .button:hover {{
                            background-color: #c0392b;
                            box-shadow: 0 6px 8px rgba(0, 0, 0, 0.3);
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
                        <h1>Seu Agendamento foi Cancelado</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                        <p>Infelizmente, seu agendamento com o barbeiro <strong>{barbeiroNome}</strong> na data <strong>{dataHora:dd/MM/yyyy - HH:mm}</strong> foi cancelado.</p>
                        <p>Por favor, entre em contato com a barbearia <strong>{nomeBarbearia}</strong> para mais informações ou para reagendar seu atendimento.</p>
                        <div class='details'>
                            <p>Pedimos desculpas por qualquer inconveniente e agradecemos pela sua compreensão.</p>
                        </div>
                        <div class='cta'>
                            <a href='{urlReagendar}' class='button'>Reagendar Agora</a>
                        </div>
                        <p>Se precisar de ajuda, entre em contato conosco diretamente pelo nosso <a href='mailto:suporte@{nomeBarbearia.ToLower().Replace(" ", "")}.com'>e-mail de suporte</a>.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                        var plainTextContent = $@"
                Olá, {destinatarioNome},

                Infelizmente, seu agendamento com o barbeiro {barbeiroNome} na data {dataHora:dd/MM/yyyy - HH:mm} foi cancelado.

                Por favor, entre em contato com a barbearia {nomeBarbearia} para mais informações ou para reagendar seu atendimento.

                Pedimos desculpas por qualquer inconveniente e agradecemos pela sua compreensão.

                Para reagendar, visite: {urlReagendar}

                Se precisar de ajuda, entre em contato conosco pelo e-mail suporte@{nomeBarbearia.ToLower().Replace(" ", "")}.com.

                Atenciosamente,
                {nomeBarbearia}

                © {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de cancelamento, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de cancelamento enviado com sucesso para {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de cancelamento para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }




    }

}

