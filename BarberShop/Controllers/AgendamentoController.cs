using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class AgendamentoController : BaseController
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IBarbeariaRepository _barbeariaRepository;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IServicoRepository _servicoRepository;
        private readonly IClienteService _clienteService;
        private readonly IEmailService _emailService;
        private readonly IPaymentService _paymentService;
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly IConfiguration _configuration;

        public AgendamentoController(
                IAgendamentoRepository agendamentoRepository,
                IClienteRepository clienteRepository,
                IBarbeiroRepository barbeiroRepository,
                IAgendamentoService agendamentoService,
                IBarbeiroService barbeiroService,
                IBarbeariaRepository barbeariaRepository,
                IServicoRepository servicoRepository,
                IClienteService clienteService,
                IEmailService emailService,
                IPaymentService paymentService,
                IPagamentoRepository pagamentoRepository,
                ILogService logService,
                IConfiguration configuration
            ) : base(logService)
        {
            _agendamentoRepository = agendamentoRepository;
            _clienteRepository = clienteRepository;
            _barbeiroRepository = barbeiroRepository;
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            _barbeariaRepository = barbeariaRepository;
            _servicoRepository = servicoRepository;
            _clienteService = clienteService;
            _emailService = emailService;
            _paymentService = paymentService;
            _pagamentoRepository = pagamentoRepository;
            _configuration = configuration;
        }


        public async Task<IActionResult> Index(string clienteNome = null, string barbeiroNome = null, int? barbeiroId = null, StatusAgendamento? status = null, StatusPagamento? statusPagamento = null,
                                       DateTime? dataInicio = null, DateTime? dataFim = null, int page = 1, int pageSize = 10, int? agendamentoId = null)
        {
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId") ?? 0;

            if (barbeariaId == 0)
            {
                return Unauthorized("Barbearia não encontrada.");
            }

            await LogAsync("INFO", nameof(Index), "Index accessed", $"Listando agendamentos da barbearia: {barbeariaId}");

            // Filtrar pelo agendamentoId se fornecido
            var agendamentos = agendamentoId.HasValue
                ? await _agendamentoService.FiltrarAgendamentosAsync(
                    agendamentoId: agendamentoId.Value,
                    barbeiroId: null,
                    barbeariaId: barbeariaId,
                    clienteNome: null,
                    dataInicio: null,
                    dataFim: null,
                    formaPagamento: null,
                    status: null,
                    statusPagamento: null,
                    barbeiroNome: null
                )
                : await _agendamentoService.FiltrarAgendamentosAsync(
                    barbeiroId: barbeiroId,
                    barbeariaId: barbeariaId,
                    clienteNome: clienteNome,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    formaPagamento: null,
                    status: status,
                    statusPagamento: statusPagamento,
                    barbeiroNome: barbeiroNome
                );

            var totalCount = agendamentos.Count();

            // Paginação
            var pagedAgendamentos = agendamentos
                .OrderByDescending(a => a.DataHora)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var barbeiros = await _barbeiroRepository.GetAllByBarbeariaIdAsync(barbeariaId);
            ViewBag.Barbeiros = barbeiros;

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(pagedAgendamentos);
        }


        [HttpGet]
        public async Task<IActionResult> FiltrarAgendamentos(string clienteNome = null,string barbeiroNome = null,int? barbeiroId = null,StatusAgendamento? status = null,StatusPagamento? statusPagamento = null,DateTime? dataInicio = null,DateTime? dataFim = null,
                                                             string formaPagamento = null, int page = 1,int pageSize = 10)
        {
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId") ?? 0;

            if (barbeariaId == 0)
            {
                return Unauthorized(new { success = false, message = "Barbearia não encontrada." });
            }

            try
            {
                // Busca agendamentos com filtros
                var agendamentos = await _agendamentoService.FiltrarAgendamentosAsync(
                    barbeiroId: barbeiroId,
                    barbeariaId: barbeariaId,
                    clienteNome: clienteNome,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    formaPagamento: formaPagamento, // Passa o parâmetro para o serviço
                    status: status,
                    statusPagamento: statusPagamento,
                    barbeiroNome: barbeiroNome
                );

                if (!agendamentos.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Nenhum agendamento encontrado com os filtros aplicados.",
                        totalCount = 0,
                        page = page,
                        pageSize = pageSize,
                        agendamentos = Array.Empty<object>()
                    });
                }

                // Total de registros
                var totalCount = agendamentos.Count();

                // Paginação
                var pagedAgendamentos = agendamentos
                    .OrderByDescending(a => a.DataHora)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Criação do retorno
                var response = new
                {
                    success = true,
                    message = "Agendamentos filtrados com sucesso.",
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    agendamentos = pagedAgendamentos.Select(a => new
                    {
                        agendamentoId = a.AgendamentoId,
                        cliente = new { nome = a.Cliente.Nome },
                        barbeiro = new { nome = a.Barbeiro?.Nome ?? "Sem barbeiro" },
                        dataHora = a.DataHora.ToString("o"),
                        status = a.Status,
                        pagamento = a.Pagamento != null ? new
                        {
                            statusPagamento = a.Pagamento.StatusPagamento,
                            valorPago = a.Pagamento.ValorPago
                        } : null,
                        formaPagamento = a.FormaPagamento,
                        precoTotal = a.PrecoTotal
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(FiltrarAgendamentos), "Erro ao filtrar agendamentos.", ex.Message);
                return StatusCode(500, new { success = false, message = "Erro interno ao processar a requisição." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ObterEventosCalendario(string clienteNome = null, string barbeiroNome = null,int? barbeiroId = null,StatusAgendamento? status = null,StatusPagamento? statusPagamento = null,DateTime? dataInicio = null,
                                                                DateTime? dataFim = null,string formaPagamento = null,int? agendamentoId = null)
        {
            var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId") ?? 0;

            if (barbeariaId == 0)   
            {
                return Unauthorized("Barbearia não encontrada.");
            }

            var agendamentos = await _agendamentoService.FiltrarAgendamentosAsync(
                barbeiroId: barbeiroId,
                barbeariaId: barbeariaId,
                clienteNome: clienteNome,
                barbeiroNome: barbeiroNome,
                dataInicio: dataInicio,
                dataFim: dataFim,
                formaPagamento: formaPagamento,
                status: status,
                statusPagamento: statusPagamento,
                agendamentoId: agendamentoId
            );

            var eventos = agendamentos.Select(a => new
            {
                id = a.AgendamentoId,
                title = $"{a.Cliente.Nome} - {a.Barbeiro?.Nome ?? "Sem barbeiro"}",
                start = a.DataHora.ToString("o"),
                end = a.DataHora.AddMinutes(a.DuracaoTotal ?? 30).ToString("o"), // Assume 30 minutos se a duração for nula
                color = a.Status == StatusAgendamento.Confirmado ? "green" : "red",
                barbeiro = a.Barbeiro != null ? new
                {
                    a.Barbeiro.BarbeiroId,
                    a.Barbeiro.Nome
                } : null
            });

            return Json(eventos);
        }




        public async Task<IActionResult> Details(int id)
        {
            await LogAsync("INFO", nameof(Details), "Detalhes do agendamento acessado", $"ID do Agendamento: {id}");
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null)
            {
                await LogAsync("WARNING", nameof(Details), "Agendamento não encontrado", $"ID: {id}");
                return NotFound();
            }

            return Json(agendamento, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            });
        }



        public async Task<IActionResult> CreateAsync()
        {
            await LogAsync("INFO", nameof(Create), "Tela de criação de agendamento acessada", "");
            ViewBag.Clientes = _clienteRepository.GetAllAsync();
            ViewBag.Barbeiros = _barbeiroRepository.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Agendamento agendamento)
        {
            var barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");

            if (ModelState.IsValid)
            {
                agendamento.BarbeariaId = barbeariaId; // Definindo o barbeariaId no agendamento
                await _agendamentoRepository.AddAsync(agendamento);
                await LogAsync("INFO", nameof(Create), "Agendamento criado com sucesso", $"ID do Agendamento: {agendamento.AgendamentoId}, ID da Barbearia: {barbeariaId}");
                return RedirectToAction(nameof(Index));
            }
            await LogAsync("WARNING", nameof(Create), "Erro de validação ao criar agendamento", "Dados do ModelState inválidos");
            return View(agendamento);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");

            await LogAsync("INFO", nameof(Edit), "Tela de edição de agendamento acessada", $"ID do Agendamento: {id}");
            var agendamento = await _agendamentoRepository.GetByIdAndBarbeariaIdAsync(id, barbeariaId);
            if (agendamento == null)
            {
                await LogAsync("WARNING", nameof(Edit), "Agendamento não encontrado para edição", $"ID: {id}");
                return NotFound();
            }

            ViewBag.Clientes = await _clienteRepository.GetAllByBarbeariaIdAsync(barbeariaId);
            ViewBag.Barbeiros = await _barbeiroRepository.GetAllByBarbeariaIdAsync(barbeariaId);
            return View(agendamento);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] Agendamento agendamento)
        {

            if (id != agendamento.AgendamentoId)
            {
                await LogAsync("ERROR", nameof(Edit), "ID de agendamento inconsistente ao editar", $"ID fornecido: {id}, ID do agendamento: {agendamento.AgendamentoId}");
                return Json(new { success = false, message = "ID do agendamento é inconsistente." });
            }

            try
            {
                await _agendamentoService.AtualizarAgendamentoAsync(id,agendamento);
                await LogAsync("INFO", nameof(Edit), "Agendamento editado com sucesso", $"ID do Agendamento: {agendamento.AgendamentoId}");

                // Retornar sucesso em JSON
                return Json(new { success = true, message = "Agendamento editado com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Edit), "Erro ao editar agendamento", ex.Message);
                return Json(new { success = false, message = "Ocorreu um erro ao tentar editar o agendamento. Tente novamente." });
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            await LogAsync("INFO", nameof(Delete), "Tela de exclusão de agendamento acessada", $"ID do Agendamento: {id}");
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null)
            {
                await LogAsync("WARNING", nameof(Delete), "Agendamento não encontrado para exclusão", $"ID: {id}");
                return NotFound();
            }
            return View(agendamento);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _agendamentoRepository.DeleteAsync(id);
                await LogAsync("INFO", nameof(DeleteConfirmed), "Agendamento excluído com sucesso", $"ID do Agendamento: {id}");

                return Json(new { success = true, message = "Agendamento excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(DeleteConfirmed), "Erro ao excluir agendamento", $"ID do Agendamento: {id}, Erro: {ex.Message}");
                return Json(new { success = false, message = "Erro ao excluir o agendamento. Tente novamente." });
            }
        }

        public async Task<IActionResult> Historico(int page = 1, int pageSize = 5) 
        {
            var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl") ?? "NomeBarbearia";

            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

            await LogAsync("INFO", nameof(Historico), "Acessado histórico de agendamentos do cliente", $"ID do Cliente: {clienteId}, ID da Barbearia: {barbeariaId}");

            // Obtem todos os agendamentos e aplica ordenação
            var agendamentos = await _clienteService.ObterHistoricoAgendamentosAsync(clienteId, barbeariaId);
            var totalCount = agendamentos.Count();

            // Aplica paginação
            var pagedAgendamentos = agendamentos
                .OrderByDescending(a => a.AgendamentoId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Passa os dados paginados e metadados para a View
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

            return View("HistoricoAgendamentos", pagedAgendamentos);
        }





        [HttpGet]
        public async Task<IActionResult> ObterHorariosDisponiveis(int barbeiroId, int duracaoTotal)
        {
            await LogAsync("INFO", nameof(ObterHorariosDisponiveis), "Solicitação de horários disponíveis", $"ID do Barbeiro: {barbeiroId}, Duração Total: {duracaoTotal}");

            if (duracaoTotal <= 0)
            {
                await LogAsync("ERROR", nameof(ObterHorariosDisponiveis), "Duração inválida para horários disponíveis", $"Duração fornecida: {duracaoTotal}");
                return BadRequest("A duração dos serviços é inválida.");
            }

            // Recupera o barbeariaId da sessão para obter o horário de funcionamento
            int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
            if (!barbeariaId.HasValue)
            {
                return BadRequest("Erro ao identificar a barbearia.");
            }

            try
            {
                // Obter os horários disponíveis e o horário de funcionamento
                var (horariosDisponiveis, horarioFuncionamento) = await _agendamentoService.ObterHorariosDisponiveisAsync(
                    barbeariaId.Value,
                    barbeiroId,
                    DateTime.Now,
                    duracaoTotal
                );

                // Transformar o horário de funcionamento em formato serializável
                var horarioFuncionamentoSerializavel = horarioFuncionamento.ToDictionary(
                    h => h.Key.ToString(),
                    h => new { Abertura = h.Value.abertura.ToString(@"hh\:mm"), Fechamento = h.Value.fechamento.ToString(@"hh\:mm") }
                );

                // Retornar JSON com os horários disponíveis e o horário de funcionamento
                return Json(new
                {
                    HorariosDisponiveis = horariosDisponiveis,
                    HorarioFuncionamento = horarioFuncionamentoSerializavel
                });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ObterHorariosDisponiveis), "Erro ao obter horários disponíveis", ex.Message);
                return StatusCode(500, "Erro interno ao obter horários disponíveis.");
            }
        }



        public async Task<IActionResult> ResumoAgendamento(int barbeiroId, DateTime dataHora, string servicoIds, string barbeariaUrl, int barbeariaId)
        {
            await LogAsync("INFO", nameof(ResumoAgendamento), "Resumo de agendamento acessado", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}, Serviços: {servicoIds}, BarbeariaUrl: {barbeariaUrl}, BarbeariaId: {barbeariaId}");

            var barbeiro = await _barbeiroService.ObterBarbeiroPorIdAsync(barbeiroId);
            if (barbeiro == null)
            {
                await LogAsync("WARNING", nameof(ResumoAgendamento), "Barbeiro não encontrado para o resumo do agendamento", $"ID do Barbeiro: {barbeiroId}");
                return NotFound("Barbeiro não encontrado.");
            }

            var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
            var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
            var precoTotal = servicos.Sum(s => s.Preco);

            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);

            var resumoAgendamentoDTO = new ResumoAgendamentoDTO
            {
                NomeBarbeiro = barbeiro.Nome,
                BarbeiroId = barbeiro.BarbeiroId,
                DataHora = dataHora,
                ServicosSelecionados = servicos.Select(s => new ServicoDTO
                {
                    ServicoId = s.ServicoId,
                    Nome = s.Nome,
                    Preco = (decimal)s.Preco
                }).ToList(),
                PrecoTotal = (decimal)precoTotal,
                FormaPagamento = "",
                BarbeariaUrl = barbeariaUrl,
                BarbeariaId = barbeariaId
            };

            if (cliente != null)
            {
                ViewData["ClienteNome"] = cliente.Nome;
                ViewData["ClienteEmail"] = cliente.Email;
            }

            ViewData["CurrentStep"] = 3;

            // Passar a PublishableKey do Stripe para a View
            ViewData["StripePublishableKey"] = Environment.GetEnvironmentVariable("Stripe__PublishableKey")
                                               ?? _configuration["Stripe:PublishableKey"];

            return View("ResumoAgendamento", resumoAgendamentoDTO);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmarAgendamento(int barbeariaId, int barbeiroId, DateTime dataHora, string servicoIds, string formaPagamento)
        {
            await LogAsync("INFO", nameof(ConfirmarAgendamento), "Iniciando confirmação do agendamento", $"ID da Barbearia: {barbeariaId}, ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}, Serviços: {servicoIds}");

            try
            {
                // Obtém a barbearia pelo ID
                var barbearia = await _barbeariaRepository.GetByIdAsync(barbeariaId);
                if (barbearia == null)
                {
                    await LogAsync("ERROR", nameof(ConfirmarAgendamento), "Barbearia não encontrada", $"ID da Barbearia: {barbeariaId}");
                    return Json(new { success = false, message = "Barbearia não encontrada." });
                }

                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (string.IsNullOrEmpty(servicoIds))
                {
                    await LogAsync("WARNING", nameof(ConfirmarAgendamento), "Nenhum serviço selecionado", $"ID do Cliente: {clienteId}");
                    return Json(new { success = false, message = "Nenhum serviço selecionado." });
                }

                var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
                var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
                var precoTotal = servicos.Sum(s => s.Preco);
                var duracaoTotal = servicos.Sum(s => s.Duracao);

                var horarioDisponivel = await _barbeiroService.VerificarDisponibilidadeHorarioAsync(barbeiroId, dataHora, duracaoTotal);
                if (!horarioDisponivel)
                {
                    await LogAsync("WARNING", nameof(ConfirmarAgendamento), "Horário indisponível", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}");
                    return Json(new { success = false, message = "O horário selecionado não está mais disponível. Por favor, escolha outro horário." });
                }

                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);

                if (cliente == null || barbeiro == null)
                {
                    await LogAsync("ERROR", nameof(ConfirmarAgendamento), "Erro ao obter cliente ou barbeiro", $"ID do Cliente: {clienteId}, ID do Barbeiro: {barbeiroId}");
                    return Json(new { success = false, message = "Erro ao obter informações do cliente ou barbeiro." });
                }

                // Criação do agendamento com barbeariaId
                var agendamentoId = await _agendamentoService.CriarAgendamentoAsync(
                    barbeariaId, barbeiroId, dataHora, clienteId, servicoIdList, formaPagamento, (decimal)precoTotal);

                // Envio de e-mails após a criação do agendamento
                var dataHoraFim = dataHora.AddMinutes(duracaoTotal);
                var tituloEvento = $"Agendamento na {barbearia.Nome}";
                var descricaoEvento = $"Agendamento com o barbeiro {barbeiro.Nome} para os serviços: {string.Join(", ", servicos.Select(s => s.Nome))}";
                var localEvento = barbearia.Endereco;
                var googleCalendarLink = _emailService.GerarLinkGoogleCalendar(tituloEvento, dataHora, dataHoraFim, descricaoEvento, localEvento);

                var assuntoCliente = $"Confirmação de Agendamento - {barbearia.Nome}";
                var conteudoCliente = "Seu agendamento foi confirmado com sucesso!";
                await _emailService.EnviarEmailAgendamentoAsync(
                    cliente.Email,
                    cliente.Nome,
                    assuntoCliente,
                    conteudoCliente,
                    barbeiro.Nome,
                    dataHora,
                    dataHoraFim,
                    (decimal)precoTotal,
                    formaPagamento,
                    barbearia.Nome,
                    googleCalendarLink
                );
                await LogAsync("INFO", nameof(ConfirmarAgendamento), "Email de confirmação enviado ao cliente", $"Email do Cliente: {cliente.Email}");

                var assuntoBarbeiro = $"Novo Agendamento - {barbearia.Nome}";
                var servicoNomes = servicos.Select(s => s.Nome).ToList();
                await _emailService.EnviarEmailNotificacaoBarbeiroAsync(
                    barbeiro.Email,
                    barbeiro.Nome,
                    cliente.Nome,
                    servicoNomes,
                    dataHora,
                    dataHoraFim,
                    (decimal)precoTotal,
                    formaPagamento,
                    barbearia.Nome
                );
                await LogAsync("INFO", nameof(ConfirmarAgendamento), "Email de notificação enviado ao barbeiro", $"Email do Barbeiro: {barbeiro.Email}");

                await LogAsync("INFO", nameof(ConfirmarAgendamento), "Agendamento confirmado com sucesso", $"ID do Agendamento: {agendamentoId}");

                return Json(new { success = true, message = "Agendamento confirmado com sucesso!", agendamentoId = agendamentoId });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ConfirmarAgendamento), $"Erro ao confirmar agendamento: {ex.Message}", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}, Serviços: {servicoIds}");
                return Json(new { success = false, message = "Ocorreu um erro ao confirmar o agendamento. Tente novamente." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> AtualizarStatusPagamento(int agendamentoId, StatusPagamento statusPagamento, string paymentId = null)
        {
            await LogAsync("INFO", nameof(AtualizarStatusPagamento), "Iniciando atualização de status de pagamento", $"ID do Agendamento: {agendamentoId}, Status: {statusPagamento}");

            try
            {
                // Obtém o pagamento usando o AgendamentoId
                var pagamentos = await _pagamentoRepository.GetPagamentosPorAgendamentoIdAsync(agendamentoId);

                if (pagamentos == null || !pagamentos.Any())
                {
                    await LogAsync("WARNING", nameof(AtualizarStatusPagamento), "Pagamento não encontrado", $"ID do Agendamento: {agendamentoId}");
                    return Json(new { success = false, message = "Pagamento não encontrado." });
                }

                // Atualiza o primeiro pagamento encontrado
                var pagamentoAtual = pagamentos.First();
                pagamentoAtual.StatusPagamento = statusPagamento;

                if (!string.IsNullOrEmpty(paymentId))
                {
                    pagamentoAtual.PaymentId = paymentId;
                    pagamentoAtual.DataPagamento = DateTime.Now;
                }

                await _pagamentoRepository.UpdateAsync(pagamentoAtual);

                await LogAsync("INFO", nameof(AtualizarStatusPagamento), "Status de pagamento atualizado com sucesso", $"ID do Agendamento: {agendamentoId}");
                return Json(new { success = true, message = "Status de pagamento atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(AtualizarStatusPagamento), $"Erro ao atualizar status de pagamento: {ex.Message}", $"ID do Agendamento: {agendamentoId}");
                return Json(new { success = false, message = "Ocorreu um erro ao atualizar o status do pagamento. Tente novamente." });
            }
        }

        public async Task<IActionResult> ObterAgendamentosPorData(DateTime dataInicio, DateTime? dataFim = null)
        {
            try
            {
                // Recupera o barbeariaId da sessão
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Barbearia não encontrada na sessão." });
                }

                // Obtém os agendamentos da barbearia dentro do período especificado
                var agendamentos = await _agendamentoRepository.GetAgendamentosPorPeriodoAsync(barbeariaId.Value, dataInicio, dataFim ?? DateTime.Now);

                // Mapeia os agendamentos para o DTO AgendamentoDto
                var agendamentosDTO = agendamentos.Select(a => new AgendamentoDto
                {
                    AgendamentoId = a.AgendamentoId,
                    DataHora = a.DataHora,
                    Status = a.Status,
                    DuracaoTotal = a.DuracaoTotal,
                    FormaPagamento = a.FormaPagamento,
                    PrecoTotal = a.AgendamentoServicos.Sum(agendamentoServico => (decimal)agendamentoServico.Servico.Preco), // Conversão explícita para decimal
                    Cliente = new ClienteDTO
                    {
                        Nome = a.Cliente.Nome,
                        Email = a.Cliente.Email,
                        Telefone = a.Cliente.Telefone
                    },
                    BarbeiroNome = a.Barbeiro.Nome,
                    StatusPagamento = a.Pagamento?.StatusPagamento.ToString() ?? "Não Pago",
                    Servicos = a.AgendamentoServicos.Select(agendamentoServico => new ServicoDTO
                    {
                        ServicoId = agendamentoServico.Servico.ServicoId,
                        Nome = agendamentoServico.Servico.Nome,
                        Preco = (decimal)agendamentoServico.Servico.Preco // Conversão explícita para decimal
                    }).ToList()
                }).ToList();

                return Json(agendamentosDTO);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ObterAgendamentosPorData), $"Erro ao buscar agendamentos: {ex.Message}", $"Data Início: {dataInicio}, Data Fim: {dataFim}");
                return Json(new { success = false, message = "Erro ao buscar agendamentos." });
            }
        }




        [HttpPost]
        public async Task<IActionResult> Inserir(int agendamentoId, decimal valorPago)
        {
            if (agendamentoId <= 0 || valorPago <= 0)
            {
                return BadRequest("AgendamentoId ou ValorPago inválido.");
            }

            try
            {
                // Obter o agendamento com base no agendamentoId
                var agendamento = await _agendamentoRepository.GetByIdAsync(agendamentoId);
                if (agendamento == null)
                {
                    return NotFound("Agendamento não encontrado.");
                }

                // Criar novo pagamento para o agendamento
                var pagamento = new Pagamento
                {
                    AgendamentoId = agendamentoId,
                    ClienteId = agendamento.ClienteId,
                    ValorPago = valorPago,
                    StatusPagamento = StatusPagamento.Aprovado,
                    DataPagamento = DateTime.Now
                };

                // Salvar o pagamento no repositório
                await _pagamentoRepository.AddAsync(pagamento);

                await LogAsync("INFO", nameof(PagamentoController), "Pagamento manual inserido com sucesso", $"Agendamento ID: {agendamentoId}, Valor Pago: {valorPago}");
                return Json(new { success = true, message = "Pagamento inserido com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(PagamentoController), $"Erro ao inserir pagamento: {ex.Message}", $"Agendamento ID: {agendamentoId}");
                return Json(new { success = false, message = "Erro ao inserir pagamento." });
            }
        }




    }
}
