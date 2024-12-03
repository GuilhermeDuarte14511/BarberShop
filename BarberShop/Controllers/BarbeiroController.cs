using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BarberShop.Application.DTOs;
using BarberShop.Application.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace BarberShopMVC.Controllers
{
    public class BarbeiroController : BaseController
    {
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IBarbeiroServicoService _barbeiroServicoService;
        private readonly IIndisponibilidadeService _indisponibilidadeService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IServicoService _servicoService;
        private readonly IAvaliacaoService _avaliacaoService;

        public BarbeiroController(
            IBarbeiroRepository barbeiroRepository,
            IBarbeiroService barbeiroService,
            IBarbeiroServicoService barbeiroServicoService,
            IIndisponibilidadeService indisponibilidadeService,
            IAgendamentoService agendamentoService,
            IServicoService servicoService,
            IAvaliacaoService avaliacaoService,
            ILogService logService)
            : base(logService)
        {
            _barbeiroRepository = barbeiroRepository;
            _barbeiroService = barbeiroService;
            _barbeiroServicoService = barbeiroServicoService;
            _indisponibilidadeService = indisponibilidadeService;
            _agendamentoService = agendamentoService;
            _servicoService = servicoService;
            _avaliacaoService = avaliacaoService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                var barbeiros = await _barbeiroService.ObterBarbeirosPorBarbeariaIdAsync(barbeariaId.Value);
                return View(barbeiros);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.Index", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar a lista de barbeiros.");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
                if (barbeiro == null)
                {
                    return NotFound();
                }

                var servicos = await _barbeiroRepository.ObterServicosPorBarbeiroIdAsync(id);

                return Json(new
                {
                    BarbeiroId = barbeiro.BarbeiroId,
                    Nome = barbeiro.Nome,
                    Email = barbeiro.Email,
                    Telefone = barbeiro.Telefone,
                    Foto = barbeiro.Foto,
                    Servicos = servicos.Select(s => new
                    {
                        s.ServicoId,
                        s.Nome,
                        s.Preco,
                        s.Duracao
                    })
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.Details", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao carregar os detalhes do barbeiro.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Barbeiro barbeiro, IFormFile Foto)
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "ID da barbearia não encontrado" });
                }

                // Associa o barbeiro à barbearia
                barbeiro.BarbeariaId = barbeariaId.Value;

                if (Foto != null && Foto.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await Foto.CopyToAsync(ms);
                        barbeiro.Foto = ms.ToArray();
                    }
                }

                var barbeiroExistente = await _barbeiroRepository.GetByEmailOrPhoneAsync(barbeiro.Email, barbeiro.Telefone);
                if (barbeiroExistente != null)
                {
                    string mensagemErro = "Já existe um cadastro com ";
                    if (barbeiroExistente.Email == barbeiro.Email)
                        mensagemErro += "esse e-mail";
                    if (barbeiroExistente.Telefone == barbeiro.Telefone)
                        mensagemErro += mensagemErro.Contains("e-mail") ? " e telefone" : " esse telefone";

                    return Json(new { success = false, message = mensagemErro });
                }

                await _barbeiroRepository.AddAsync(barbeiro);

                return Json(new { success = true, message = "Barbeiro adicionado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao adicionar o barbeiro" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Barbeiro barbeiro)
        {
            if (id != barbeiro.BarbeiroId)
                return BadRequest(new { success = false, message = "ID do barbeiro não corresponde." });

            try
            {
                await _barbeiroRepository.UpdateAsync(barbeiro);
                return Json(new { success = true, message = "Barbeiro atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.Edit", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao atualizar o barbeiro.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
                if (barbeiro == null)
                {
                    return NotFound();
                }

                // Desvincular todos os serviços associados ao barbeiro
                var servicosVinculados = await _barbeiroServicoService.ObterServicosPorBarbeiroIdAsync(id);
                foreach (var servico in servicosVinculados)
                {
                    await _barbeiroServicoService.DesvincularServicoAsync(id, servico.ServicoId);
                }

                // Após desvincular, excluir o barbeiro
                await _barbeiroService.DeletarBarbeiroAsync(id);
                return Json(new { success = true, message = "Barbeiro excluído com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.DeleteConfirmed", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao excluir o barbeiro.");
            }
        }

        public async Task<IActionResult> EscolherBarbeiro(int duracaoTotal, string servicoIds, string barbeariaUrl)
        {
            if (duracaoTotal <= 0)
            {
                return BadRequest("A duração dos serviços é inválida.");
            }

            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                // Converter os IDs dos serviços de string para lista de inteiros
                var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();

                // Obter os barbeiros e verificar os serviços que eles realizam
                var barbeiros = await _barbeiroService.ObterBarbeirosPorServicosAsync(barbeariaId.Value, servicoIdList);

                // Passar os dados para a View
                ViewData["DuracaoTotal"] = duracaoTotal;
                ViewData["ServicoIds"] = servicoIds;
                ViewData["BarbeariaUrl"] = barbeariaUrl;
                ViewData["BarbeariaId"] = barbeariaId;
                ViewData["CurrentStep"] = 2;

                return View("EscolherBarbeiro", barbeiros);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.EscolherBarbeiro", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar os barbeiros.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFoto(int id, IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    return Json(new { success = false, message = "Nenhuma foto foi selecionada." });
                }

                var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
                if (barbeiro == null)
                {
                    return NotFound("Barbeiro não encontrado.");
                }

                using (var ms = new MemoryStream())
                {
                    await foto.CopyToAsync(ms);
                    barbeiro.Foto = ms.ToArray();
                }

                await _barbeiroRepository.UpdateAsync(barbeiro);
                var fotoBase64 = Convert.ToBase64String(barbeiro.Foto);
                return Json(new { success = true, newFotoBase64 = fotoBase64 });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.UploadFoto", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao fazer o upload da foto.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterBarbeirosPorBarbearia()
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "ID da barbearia não encontrado na sessão." });
                }

                var barbeiros = await _barbeiroService.ObterBarbeirosPorBarbeariaIdAsync(barbeariaId.Value);

                if (!barbeiros.Any())
                {
                    return Json(new { success = true, message = "Nenhum barbeiro encontrado para esta barbearia.", barbeiros = new List<object>() });
                }

                var barbeirosFormatados = barbeiros.Select(b => new
                {
                    BarbeiroId = b.BarbeiroId,
                    Nome = b.Nome
                });

                return Json(new { success = true, barbeiros = barbeirosFormatados });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.ObterBarbeirosPorBarbearia", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar os barbeiros.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DesvincularServico(int barbeiroId, int servicoId)
        {
            try
            {
                var sucesso = await _barbeiroServicoService.DesvincularServicoAsync(barbeiroId, servicoId);
                if (!sucesso)
                {
                    return Json(new { success = false, message = "Serviço não encontrado ou já desvinculado." });
                }

                return Json(new { success = true, message = "Serviço desvinculado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.DesvincularServico", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao desvincular o serviço.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterServicosNaoVinculados(int barbeiroId)
        {
            try
            {
                if (barbeiroId <= 0)
                {
                    return BadRequest(new { message = "ID do barbeiro inválido." });
                }

                var servicos = await _barbeiroServicoService.ObterServicosNaoVinculadosAsync(barbeiroId);

                return Json(servicos.Select(s => new
                {
                    s.ServicoId,
                    s.Nome,
                    s.Preco,
                    s.Duracao
                }));
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.ObterServicosNaoVinculados", ex.Message, ex.ToString(), barbeiroId.ToString());
                return StatusCode(500, new { message = "Erro ao obter os serviços não vinculados." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> VincularServico(int barbeiroId, int servicoId)
        {
            try
            {
                await _barbeiroServicoService.VincularServicoAsync(barbeiroId, servicoId);

                var servico = await _barbeiroServicoService.ObterServicoPorIdAsync(servicoId);

                // Retorne apenas os dados necessários
                return Json(new
                {
                    success = true,
                    servico = new
                    {
                        servico.ServicoId,
                        servico.Nome,
                        servico.Preco,
                        servico.Duracao
                    }
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.VincularServico", ex.Message, ex.ToString(), $"{barbeiroId}, {servicoId}");
                return StatusCode(500, new { message = "Erro ao vincular serviço." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> FiltrarAgendamentos(int page = 1, int pageSize = 10, string clienteNome = null, DateTime? dataInicio = null, DateTime? dataFim = null, string formaPagamento = null, StatusAgendamento? status = null, StatusPagamento? statusPagamento = null,
                                                             bool isAdmin = false, string barbeiroNome = null)
        {
            try
            {
                var claimsPrincipal = User;
                var barbeariaIdClaim = claimsPrincipal.FindFirst("BarbeariaId")?.Value;

                // Valida se o claim da barbearia está disponível
                if (string.IsNullOrEmpty(barbeariaIdClaim))
                {
                    return Unauthorized();
                }

                int barbeariaId = int.Parse(barbeariaIdClaim);

                // Para administradores, barbeiroId pode ser ignorado
                int? barbeiroId = null;
                if (!isAdmin)
                {
                    var barbeiroIdClaim = claimsPrincipal.FindFirst("BarbeiroId")?.Value;
                    if (string.IsNullOrEmpty(barbeiroIdClaim))
                    {
                        return Unauthorized();
                    }
                    barbeiroId = int.Parse(barbeiroIdClaim);
                }

                // Chamando o serviço para filtrar
                var agendamentos = await _agendamentoService.FiltrarAgendamentosAsync(
                    barbeiroId,
                    barbeariaId,
                    clienteNome,
                    dataInicio,
                    dataFim,
                    formaPagamento,
                    status,
                    statusPagamento,
                    barbeiroNome
                );

                // Paginação
                var totalCount = agendamentos.Count();
                var pagedAgendamentos = agendamentos
                    .OrderByDescending(a => a.DataHora)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                return Json(new
                {
                    agendamentos = pagedAgendamentos.Select(a => new
                    {
                        a.AgendamentoId,
                        Cliente = new { a.Cliente.Nome },
                        Barbeiro = a.Barbeiro != null ? new { a.Barbeiro.Nome } : null,
                        a.DataHora,
                        a.Status,
                        Pagamento = a.Pagamento != null ? new { a.Pagamento.StatusPagamento } : null,
                        a.FormaPagamento,
                        a.PrecoTotal
                    }),
                    totalCount
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(FiltrarAgendamentos), ex.Message, ex.ToString());
                return StatusCode(500, new { error = "Erro ao filtrar os agendamentos." });
            }
        }



        public async Task<IActionResult> MeusAgendamentos(int page = 1, int pageSize = 10, int? agendamentoId = null)
        {
            try
            {
                var claimsPrincipal = User;
                var barbeiroIdClaim = claimsPrincipal.FindFirst("BarbeiroId")?.Value;
                var barbeariaIdClaim = claimsPrincipal.FindFirst("BarbeariaId")?.Value;

                // Valida os claims
                if (string.IsNullOrEmpty(barbeiroIdClaim) || string.IsNullOrEmpty(barbeariaIdClaim))
                {
                    return RedirectToAction("Login", "Login");
                }

                int barbeiroId = int.Parse(barbeiroIdClaim);
                int barbeariaId = int.Parse(barbeariaIdClaim);

                var urlSlug = User.FindFirst("urlSlug")?.Value;

                if (string.IsNullOrEmpty(urlSlug))
                {
                    return Unauthorized("UrlSlug não encontrado nos claims.");
                }

                ViewData["UrlSlug"] = urlSlug;

                // Busca agendamentos com base no barbeiro, barbearia e opcionalmente no agendamentoId
                var agendamentos = await _agendamentoService.ObterAgendamentosPorBarbeiroEBarbeariaAsync(barbeiroId, barbeariaId, agendamentoId);
                var totalCount = agendamentos.Count();

                // Aplica paginação
                var pagedAgendamentos = agendamentos
                    .OrderByDescending(a => a.DataHora)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewData["CurrentPage"] = page;
                ViewData["PageSize"] = pageSize;
                ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

                return View("MeusAgendamentos", pagedAgendamentos);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.MeusAgendamentos", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar os agendamentos.");
            }
        }


        [HttpGet]
        public async Task<IActionResult> DetailsAgendamento(int id)
        {
            var agendamento = await _agendamentoService.ObterAgendamentoPorIdAsync(id);

            if (agendamento == null)
                return NotFound();

            var agendamentoDto = new AgendamentoDto
            {
                AgendamentoId = agendamento.AgendamentoId,
                DataHora = agendamento.DataHora,
                Status = agendamento.Status,
                PrecoTotal = (decimal)agendamento.PrecoTotal,
                StatusPagamento = agendamento.Pagamento?.StatusPagamento.ToString() ?? "Não Especificado",
                FormaPagamento = agendamento.FormaPagamento,
                Cliente = new ClienteDTO { Nome = agendamento.Cliente?.Nome }
            };

            return Json(agendamentoDto);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarAgendamentoMeuBarbeiro([FromBody] AgendamentoDto dto)
        {
            try
            {
                Console.WriteLine("DTO Recebido: " + System.Text.Json.JsonSerializer.Serialize(dto));

                if (!dto.AgendamentoId.HasValue || dto.AgendamentoId <= 0)
                {
                    return BadRequest(new { success = false, message = "ID do agendamento inválido." });
                }

                if (!dto.PrecoTotal.HasValue || dto.PrecoTotal <= 0)
                {
                    return BadRequest(new { success = false, message = "Preço total inválido." });
                }

                if (!dto.DataHora.HasValue)
                {
                    return BadRequest(new { success = false, message = "Data/hora inválida." });
                }

                var agendamentoExistente = await _agendamentoService.ObterAgendamentoCompletoPorIdAsync(dto.AgendamentoId.Value);
                if (agendamentoExistente == null)
                {
                    return NotFound(new { success = false, message = "Agendamento não encontrado." });
                }

                await _agendamentoService.AtualizarAgendamentoAsync(dto.AgendamentoId.Value, dto);

                return Ok(new { success = true, message = "Agendamento atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao atualizar agendamento: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Erro interno ao atualizar o agendamento." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> MeusServicos()
        {
            var claimsPrincipal = User;
            var barbeiroIdClaim = claimsPrincipal.FindFirst("BarbeiroId")?.Value;
            var barbeariaIdClaim = claimsPrincipal.FindFirst("BarbeariaId")?.Value;

            // Verifica se os claims do barbeiro estão presentes
            if (string.IsNullOrEmpty(barbeiroIdClaim) || string.IsNullOrEmpty(barbeariaIdClaim))
            {
                return RedirectToAction("Login", "Login");
            }

            int barbeiroId = int.Parse(barbeiroIdClaim);
            int barbeariaId = int.Parse(barbeariaIdClaim);

            var servicosVinculados = await _barbeiroServicoService.ObterServicosPorBarbeiroIdAsync(barbeiroId);
            var servicosNaoVinculados = await _barbeiroServicoService.ObterServicosNaoVinculadosAsync(barbeiroId);

            var model = new MeusServicosDto
            {
                BarbeiroId = barbeiroId,
                BarbeariaId = barbeariaId,
                ServicosVinculados = servicosVinculados.Select(s => new ServicoDto
                {
                    ServicoId = s.ServicoId,
                    Nome = s.Nome,
                    Preco = (decimal)s.Preco,
                    Duracao = s.Duracao
                }).ToList(),
                ServicosNaoVinculados = servicosNaoVinculados.Select(s => new ServicoDto
                {
                    ServicoId = s.ServicoId,
                    Nome = s.Nome,
                    Preco = (decimal)s.Preco,
                    Duracao = s.Duracao
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VincularServicoMeuBarbeiro([FromBody] int servicoId)
        {
            int barbeiroId = ObterBarbeiroIdLogado(); // Obtém o ID do barbeiro logado

            try
            {
                await _barbeiroServicoService.VincularServicoAsync(barbeiroId, servicoId);

                var servico = await _barbeiroServicoService.ObterServicoPorIdAsync(servicoId);

                return Json(new
                {
                    success = true,
                    message = "Serviço vinculado com sucesso!",
                    servico = new
                    {
                        servico.ServicoId,
                        servico.Nome,
                        servico.Preco,
                        servico.Duracao
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao vincular serviço: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DesvincularServicoMeuBarbeiro([FromBody] int servicoId)
        {
            int barbeiroId = ObterBarbeiroIdLogado();

            try
            {
                bool result = await _barbeiroServicoService.DesvincularServicoAsync(barbeiroId, servicoId);
                if (result)
                    return Json(new { success = true, message = "Serviço desvinculado com sucesso!" });

                return Json(new { success = false, message = "Serviço não encontrado para desvincular." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao desvincular serviço: {ex.Message}" });
            }
        }


        public async Task<IActionResult> MeusDadosBarbeiro()
        {
            try
            {

                var barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    await LogAsync("WARNING", nameof(BarbeiroController), "Tentativa de acesso sem barbeiro logado", "BarbeiroId nulo ou inválido");
                    return RedirectToAction("Login", "Login");
                }

                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);
                if (barbeiro == null)
                {
                    await LogAsync("WARNING", nameof(BarbeiroController), "Barbeiro não encontrado", $"BarbeiroId: {barbeiroId}");
                    return NotFound("Barbeiro não encontrado.");
                }

                if (barbeiro.Foto != null)
                {
                    ViewData["FotoBarbeiro"] = "data:image/png;base64," + Convert.ToBase64String(barbeiro.Foto);
                }

                await LogAsync("INFO", nameof(BarbeiroController), "Acesso à página Meus Dados", $"BarbeiroId: {barbeiroId}");

                return View("MeusDadosBarbeiro", barbeiro);
            }
            catch (Exception ex)
            {
                // Log de erro com a mensagem da exceção
                await LogAsync("ERROR", nameof(BarbeiroController), "Erro ao acessar Meus Dados", ex.Message);
                return RedirectToAction("Erro", "Home"); // Redireciona para uma página de erro
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFotoMeusDadosBarbeiro(IFormFile Foto)
        {
            try
            {
                var barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0) return Json(new { success = false });

                if (Foto == null || Foto.Length == 0)
                {
                    return Json(new { success = false, message = "Nenhuma foto foi selecionada." });
                }

                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);
                if (barbeiro == null)
                {
                    return NotFound("Barbeiro não encontrado.");
                }

                using (var ms = new MemoryStream())
                {
                    await Foto.CopyToAsync(ms);
                    barbeiro.Foto = ms.ToArray();
                }

                await _barbeiroRepository.UpdateAsync(barbeiro);
                var fotoBase64 = Convert.ToBase64String(barbeiro.Foto);
                return Json(new { success = true, newFotoBase64 = fotoBase64 });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.UploadFoto", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao fazer o upload da foto.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> AtualizarMeusDados([FromBody] AtualizarBarbeiroUsuarioDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        sucesso = false,
                        mensagem = "Dados inválidos. Por favor, preencha todos os campos corretamente."
                    });
                }

                await _barbeiroService.AtualizarBarbeiroEUsuarioAsync(dto);

                return Json(new
                {
                    sucesso = true,
                    mensagem = "Dados atualizados com sucesso!"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new
                {
                    sucesso = false,
                    mensagem = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sucesso = false,
                    mensagem = "Erro interno ao atualizar os dados. Tente novamente mais tarde."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MeusHorarios()
        {
            try
            {
                int barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    return RedirectToAction("Login", "Login");
                }

                var indisponibilidades = await _indisponibilidadeService.ObterIndisponibilidadesPorBarbeiroAsync(barbeiroId);
                return View("MeusHorarios", indisponibilidades);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.MeusHorarios", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar os horários.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarHorario([FromBody] IndisponibilidadeBarbeiro indisponibilidade)
        {
            try
            {
                int barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    return Unauthorized("Barbeiro não autenticado.");
                }

                if (indisponibilidade.DataInicio >= indisponibilidade.DataFim)
                {
                    return BadRequest(new { success = false, message = "A data de início deve ser anterior à data de fim." });
                }

                indisponibilidade.BarbeiroId = barbeiroId;

                await _indisponibilidadeService.AdicionarIndisponibilidadeAsync(indisponibilidade);
                return Json(new { success = true, message = "Horário adicionado com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.AdicionarHorario", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao adicionar o horário.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarHorario([FromBody] IndisponibilidadeBarbeiro indisponibilidade)
        {
            try
            {
                int barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    return Unauthorized("Barbeiro não autenticado.");
                }

                if (indisponibilidade.DataInicio >= indisponibilidade.DataFim)
                {
                    return BadRequest(new { success = false, message = "A data de início deve ser anterior à data de fim." });
                }

                var horarioExistente = await _indisponibilidadeService.ObterPorIdAsync(indisponibilidade.IndisponibilidadeId);
                if (horarioExistente == null || horarioExistente.BarbeiroId != barbeiroId)
                {
                    return Unauthorized("Horário não pertence ao barbeiro logado.");
                }

                await _indisponibilidadeService.AtualizarIndisponibilidadeAsync(indisponibilidade);
                return Json(new { success = true, message = "Horário atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.EditarHorario", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao editar o horário.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirHorario([FromBody] int id)
        {
            try
            {
                int barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    return Unauthorized("Barbeiro não autenticado.");
                }

                var horario = await _indisponibilidadeService.ObterPorIdAsync(id);
                if (horario == null || horario.BarbeiroId != barbeiroId)
                {
                    return Unauthorized("Horário não pertence ao barbeiro logado.");
                }

                await _indisponibilidadeService.ExcluirIndisponibilidadeAsync(id);
                return Json(new { success = true, message = "Horário excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.ExcluirHorario", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao excluir o horário.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MinhasAvaliacoes(int page = 1, int pageSize = 10, int? avaliacaoId = null, string? dataInicio = null, string? dataFim = null, int? notaServico = null, int? notaBarbeiro = null, string? observacao = null)
        {
            try
            {
                // Obtém o ID do barbeiro logado
                var barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    return RedirectToAction("Login", "Login");
                }

                // Obtém o ID da barbearia da sessão
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return RedirectToAction("Login", "Login");
                }

                // Busca avaliações com filtros aplicados
                var avaliacoes = await _avaliacaoService.ObterAvaliacoesFiltradasAsync(
                    barbeariaId: barbeariaId.Value,
                    barbeiroId: barbeiroId,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    notaServico: notaServico,
                    notaBarbeiro: notaBarbeiro,
                    observacao: observacao);

                // Total de registros para paginação
                var totalCount = avaliacoes.Count();

                // Paginação
                var pagedAvaliacoes = avaliacoes
                    .OrderByDescending(a => a.DataAvaliado)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AvaliacaoDTO
                    {
                        AvaliacaoId = a.AvaliacaoId,
                        AgendamentoId = a.AgendamentoId,
                        NotaBarbeiro = a.NotaBarbeiro,
                        NotaServico = a.NotaServico,
                        Observacao = a.Observacao,
                        DataAvaliado = a.DataAvaliado
                    })
                    .ToList();

                // Adiciona dados de paginação ao ViewData
                ViewData["CurrentPage"] = page;
                ViewData["PageSize"] = pageSize;
                ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

                return View("MinhasAvaliacoes", pagedAvaliacoes);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(BarbeiroController), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar as avaliações.");
            }
        }


        [HttpGet]
        public async Task<IActionResult> FiltrarAvaliacoes(int page = 1,int pageSize = 10,int? avaliacaoId = null, string? dataInicio = null,string? dataFim = null,int? notaServico = null,int? notaBarbeiro = null,string? observacao = null)
        {
            try
            {
                // Obtém o ID do barbeiro logado
                var barbeiroId = ObterBarbeiroIdLogado();
                if (barbeiroId <= 0)
                {
                    return Unauthorized("Usuário não autenticado.");
                }

                // Obtém o ID da barbearia da sessão
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Unauthorized("Barbearia não encontrada na sessão.");
                }

                // Busca avaliações filtradas
                var avaliacoes = await _avaliacaoService.ObterAvaliacoesFiltradasAsync(
                    barbeariaId: barbeariaId.Value,
                    barbeiroId: barbeiroId,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    notaServico: notaServico,
                    notaBarbeiro: notaBarbeiro,
                    observacao: observacao);

                // Total de registros
                var totalCount = avaliacoes.Count();

                // Paginação
                var pagedAvaliacoes = avaliacoes
                    .OrderByDescending(a => a.DataAvaliado)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AvaliacaoDTO
                    {
                        AvaliacaoId = a.AvaliacaoId,
                        AgendamentoId = a.AgendamentoId,
                        NotaBarbeiro = a.NotaBarbeiro,
                        NotaServico = a.NotaServico,
                        Observacao = a.Observacao,
                        DataAvaliado = a.DataAvaliado
                    })
                    .ToList();

                return Json(new
                {
                    avaliacoes = pagedAvaliacoes,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(FiltrarAvaliacoes), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao filtrar as avaliações.");
            }
        }






    }
}
