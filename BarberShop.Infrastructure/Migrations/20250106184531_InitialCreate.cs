using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Barbearias",
                columns: table => new
                {
                    BarbeariaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlSlug = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CEP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HorarioFuncionamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountIdStripe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barbearias", x => x.BarbeariaId);
                });

            migrationBuilder.CreateTable(
                name: "FeriadosNacionais",
                columns: table => new
                {
                    FeriadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Recorrente = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeriadosNacionais", x => x.FeriadoId);
                });

            migrationBuilder.CreateTable(
                name: "GraficoPosicao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    GraficoId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Posicao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraficoPosicao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LogLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosAssinaturasSite",
                columns: table => new
                {
                    AssinaturaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    NomeCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelefoneCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusPagamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosAssinaturasSite", x => x.AssinaturaId);
                });

            migrationBuilder.CreateTable(
                name: "PlanoAssinaturaSistema",
                columns: table => new
                {
                    PlanoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdProdutoStripe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Periodicidade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoAssinaturaSistema", x => x.PlanoId);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Barbeiros",
                columns: table => new
                {
                    BarbeiroId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barbeiros", x => x.BarbeiroId);
                    table.ForeignKey(
                        name: "FK_Barbeiros_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoValidacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoValidacaoExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenRecuperacaoSenha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClienteId);
                    table.ForeignKey(
                        name: "FK_Clientes_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeriadosBarbearias",
                columns: table => new
                {
                    FeriadoBarbeariaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Recorrente = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeriadosBarbearias", x => x.FeriadoBarbeariaId);
                    table.ForeignKey(
                        name: "FK_FeriadosBarbearias_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanoAssinaturaBarbearias",
                columns: table => new
                {
                    PlanoBarbeariaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Periodicidade = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoAssinaturaBarbearias", x => x.PlanoBarbeariaId);
                    table.ForeignKey(
                        name: "FK_PlanoAssinaturaBarbearias_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Preco = table.Column<float>(type: "real", nullable: false),
                    Duracao = table.Column<int>(type: "int", nullable: false),
                    BarbeariaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.ServicoId);
                    table.ForeignKey(
                        name: "FK_Servicos_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CodigoValidacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoValidacaoExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenRecuperacaoSenha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false),
                    BarbeiroId = table.Column<int>(type: "int", nullable: true),
                    OnboardingCompleto = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndisponibilidadesBarbeiros",
                columns: table => new
                {
                    IndisponibilidadeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndisponibilidadesBarbeiros", x => x.IndisponibilidadeId);
                    table.ForeignKey(
                        name: "FK_IndisponibilidadesBarbeiros_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "BarbeiroId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    AgendamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DuracaoTotal = table.Column<int>(type: "int", nullable: true),
                    FormaPagamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrecoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false),
                    EmailEnviado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.AgendamentoId);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "BarbeiroId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BarbeiroServicos",
                columns: table => new
                {
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarbeiroServicos", x => new { x.BarbeiroId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_BarbeiroServicos_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "BarbeiroId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BarbeiroServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanoBeneficios",
                columns: table => new
                {
                    PlanoBeneficioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanoBarbeariaId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoBeneficios", x => x.PlanoBeneficioId);
                    table.ForeignKey(
                        name: "FK_PlanoBeneficios_PlanoAssinaturaBarbearias_PlanoBarbeariaId",
                        column: x => x.PlanoBarbeariaId,
                        principalTable: "PlanoAssinaturaBarbearias",
                        principalColumn: "PlanoBarbeariaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanoBeneficios_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingProgresso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tela = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Concluido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingProgresso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingProgresso_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatoriosPersonalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoRelatorio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodoDias = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Configuracoes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatoriosPersonalizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatoriosPersonalizados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendamentoServicos",
                columns: table => new
                {
                    AgendamentoId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendamentoServicos", x => new { x.AgendamentoId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_AgendamentoServicos_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendamentoServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacao",
                columns: table => new
                {
                    AvaliacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgendamentoId = table.Column<int>(type: "int", nullable: false),
                    NotaBarbeiro = table.Column<int>(type: "int", nullable: false),
                    NotaServico = table.Column<int>(type: "int", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataAvaliado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacao", x => x.AvaliacaoId);
                    table.ForeignKey(
                        name: "FK_Avaliacao_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    NotificacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    BarbeariaId = table.Column<int>(type: "int", nullable: false),
                    AgendamentoId = table.Column<int>(type: "int", nullable: true),
                    Mensagem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Lida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.NotificacaoId);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagamentos",
                columns: table => new
                {
                    PagamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgendamentoId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StatusPagamento = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BarbeariaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamentos", x => x.PagamentoId);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Barbearias_BarbeariaId",
                        column: x => x.BarbeariaId,
                        principalTable: "Barbearias",
                        principalColumn: "BarbeariaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_BarbeariaId",
                table: "Agendamentos",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_BarbeiroId",
                table: "Agendamentos",
                column: "BarbeiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ClienteId",
                table: "Agendamentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoServicos_ServicoId",
                table: "AgendamentoServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_AgendamentoId",
                table: "Avaliacao",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Barbearias_UrlSlug",
                table: "Barbearias",
                column: "UrlSlug",
                unique: true,
                filter: "[UrlSlug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Barbeiros_BarbeariaId",
                table: "Barbeiros",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_BarbeiroServicos_ServicoId",
                table: "BarbeiroServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_BarbeariaId",
                table: "Clientes",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_FeriadosBarbearias_BarbeariaId",
                table: "FeriadosBarbearias",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_IndisponibilidadesBarbeiros_BarbeiroId",
                table: "IndisponibilidadesBarbeiros",
                column: "BarbeiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_AgendamentoId",
                table: "Notificacoes",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_BarbeariaId",
                table: "Notificacoes",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_UsuarioId",
                table: "Notificacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingProgresso_UsuarioId",
                table: "OnboardingProgresso",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_AgendamentoId",
                table: "Pagamentos",
                column: "AgendamentoId",
                unique: true,
                filter: "[AgendamentoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_BarbeariaId",
                table: "Pagamentos",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_ClienteId",
                table: "Pagamentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoAssinaturaBarbearias_BarbeariaId",
                table: "PlanoAssinaturaBarbearias",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoBeneficios_PlanoBarbeariaId",
                table: "PlanoBeneficios",
                column: "PlanoBarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoBeneficios_ServicoId",
                table: "PlanoBeneficios",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatoriosPersonalizados_UsuarioId",
                table: "RelatoriosPersonalizados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_BarbeariaId",
                table: "Servicos",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_BarbeariaId",
                table: "Usuarios",
                column: "BarbeariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendamentoServicos");

            migrationBuilder.DropTable(
                name: "Avaliacao");

            migrationBuilder.DropTable(
                name: "BarbeiroServicos");

            migrationBuilder.DropTable(
                name: "FeriadosBarbearias");

            migrationBuilder.DropTable(
                name: "FeriadosNacionais");

            migrationBuilder.DropTable(
                name: "GraficoPosicao");

            migrationBuilder.DropTable(
                name: "IndisponibilidadesBarbeiros");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "OnboardingProgresso");

            migrationBuilder.DropTable(
                name: "Pagamentos");

            migrationBuilder.DropTable(
                name: "PagamentosAssinaturasSite");

            migrationBuilder.DropTable(
                name: "PlanoAssinaturaSistema");

            migrationBuilder.DropTable(
                name: "PlanoBeneficios");

            migrationBuilder.DropTable(
                name: "PushSubscriptions");

            migrationBuilder.DropTable(
                name: "RelatoriosPersonalizados");

            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropTable(
                name: "PlanoAssinaturaBarbearias");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Barbeiros");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Barbearias");
        }
    }
}
