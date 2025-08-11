using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BarberShop.Infrastructure.MigrationsPostgres
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "barbearias",
                columns: table => new
                {
                    barbearia_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: true),
                    url_slug = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    endereco = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    cep = table.Column<string>(type: "text", nullable: true),
                    horario_funcionamento = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    account_id_stripe = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    logo = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barbearias", x => x.barbearia_id);
                });

            migrationBuilder.CreateTable(
                name: "feriados_nacionais",
                columns: table => new
                {
                    feriado_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    recorrente = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feriados_nacionais", x => x.feriado_id);
                });

            migrationBuilder.CreateTable(
                name: "grafico_posicao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    grafico_id = table.Column<string>(type: "text", nullable: true),
                    posicao = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grafico_posicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "logs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    log_date_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    log_level = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    data = table.Column<string>(type: "text", nullable: true),
                    resource_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_logs", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos_assinaturas_site",
                columns: table => new
                {
                    assinatura_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    nome_cliente = table.Column<string>(type: "text", nullable: false),
                    email_cliente = table.Column<string>(type: "text", nullable: false),
                    telefone_cliente = table.Column<string>(type: "text", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_id = table.Column<string>(type: "text", nullable: false),
                    status_pagamento = table.Column<string>(type: "text", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamentos_assinaturas_site", x => x.assinatura_id);
                });

            migrationBuilder.CreateTable(
                name: "plano_assinatura_sistema",
                columns: table => new
                {
                    plano_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    id_produto_stripe = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    periodicidade = table.Column<string>(type: "text", nullable: false),
                    price_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_assinatura_sistema", x => x.plano_id);
                });

            migrationBuilder.CreateTable(
                name: "push_subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    endpoint = table.Column<string>(type: "text", nullable: false),
                    p256dh = table.Column<string>(type: "text", nullable: false),
                    auth = table.Column<string>(type: "text", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_push_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "barbeiros",
                columns: table => new
                {
                    barbeiro_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    foto = table.Column<byte[]>(type: "bytea", nullable: true),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barbeiros", x => x.barbeiro_id);
                    table.ForeignKey(
                        name: "fk_barbeiros_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    cliente_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    senha = table.Column<string>(type: "text", nullable: true),
                    codigo_validacao = table.Column<string>(type: "text", nullable: true),
                    codigo_validacao_expiracao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    role = table.Column<string>(type: "text", nullable: false),
                    token_recuperacao_senha = table.Column<string>(type: "text", nullable: true),
                    token_expiracao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.cliente_id);
                    table.ForeignKey(
                        name: "fk_clientes_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feriados_barbearias",
                columns: table => new
                {
                    feriado_barbearia_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    recorrente = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feriados_barbearias", x => x.feriado_barbearia_id);
                    table.ForeignKey(
                        name: "fk_feriados_barbearias_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plano_assinatura_barbearias",
                columns: table => new
                {
                    plano_barbearia_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    periodicidade = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_assinatura_barbearias", x => x.plano_barbearia_id);
                    table.ForeignKey(
                        name: "fk_plano_assinatura_barbearias_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                columns: table => new
                {
                    servico_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    preco = table.Column<float>(type: "real", nullable: false),
                    duracao = table.Column<int>(type: "integer", nullable: false),
                    barbearia_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_servicos", x => x.servico_id);
                    table.ForeignKey(
                        name: "fk_servicos_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    codigo_validacao = table.Column<string>(type: "text", nullable: true),
                    codigo_validacao_expiracao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    token_recuperacao_senha = table.Column<string>(type: "text", nullable: true),
                    token_expiracao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false),
                    barbeiro_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.usuario_id);
                    table.ForeignKey(
                        name: "fk_usuarios_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indisponibilidades_barbeiros",
                columns: table => new
                {
                    indisponibilidade_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    barbeiro_id = table.Column<int>(type: "integer", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    motivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_indisponibilidades_barbeiros", x => x.indisponibilidade_id);
                    table.ForeignKey(
                        name: "fk_indisponibilidades_barbeiros_barbeiros_barbeiro_id",
                        column: x => x.barbeiro_id,
                        principalTable: "barbeiros",
                        principalColumn: "barbeiro_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agendamentos",
                columns: table => new
                {
                    agendamento_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_hora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    duracao_total = table.Column<int>(type: "integer", nullable: true),
                    forma_pagamento = table.Column<string>(type: "text", nullable: true),
                    preco_total = table.Column<decimal>(type: "numeric", nullable: true),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    barbeiro_id = table.Column<int>(type: "integer", nullable: false),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false),
                    email_enviado = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agendamentos", x => x.agendamento_id);
                    table.ForeignKey(
                        name: "fk_agendamentos_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamentos_barbeiros_barbeiro_id",
                        column: x => x.barbeiro_id,
                        principalTable: "barbeiros",
                        principalColumn: "barbeiro_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_agendamentos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "barbeiro_servicos",
                columns: table => new
                {
                    barbeiro_id = table.Column<int>(type: "integer", nullable: false),
                    servico_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barbeiro_servicos", x => new { x.barbeiro_id, x.servico_id });
                    table.ForeignKey(
                        name: "fk_barbeiro_servicos_barbeiros_barbeiro_id",
                        column: x => x.barbeiro_id,
                        principalTable: "barbeiros",
                        principalColumn: "barbeiro_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barbeiro_servicos_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "servico_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plano_beneficios",
                columns: table => new
                {
                    plano_beneficio_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plano_barbearia_id = table.Column<int>(type: "integer", nullable: false),
                    servico_id = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_beneficios", x => x.plano_beneficio_id);
                    table.ForeignKey(
                        name: "fk_plano_beneficios_plano_assinatura_barbearias_plano_barbeari",
                        column: x => x.plano_barbearia_id,
                        principalTable: "plano_assinatura_barbearias",
                        principalColumn: "plano_barbearia_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plano_beneficios_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "servico_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relatorios_personalizados",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_relatorio = table.Column<string>(type: "text", nullable: false),
                    periodo_dias = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    configuracoes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relatorios_personalizados", x => x.id);
                    table.ForeignKey(
                        name: "fk_relatorios_personalizados_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agendamento_servicos",
                columns: table => new
                {
                    agendamento_id = table.Column<int>(type: "integer", nullable: false),
                    servico_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agendamento_servicos", x => new { x.agendamento_id, x.servico_id });
                    table.ForeignKey(
                        name: "fk_agendamento_servicos_agendamentos_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamentos",
                        principalColumn: "agendamento_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_agendamento_servicos_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "servico_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avaliacao",
                columns: table => new
                {
                    avaliacao_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agendamento_id = table.Column<int>(type: "integer", nullable: false),
                    nota_barbeiro = table.Column<int>(type: "integer", nullable: false),
                    nota_servico = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: false),
                    data_avaliado = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avaliacao", x => x.avaliacao_id);
                    table.ForeignKey(
                        name: "fk_avaliacao_agendamentos_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamentos",
                        principalColumn: "agendamento_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notificacoes",
                columns: table => new
                {
                    notificacao_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    barbearia_id = table.Column<int>(type: "integer", nullable: false),
                    agendamento_id = table.Column<int>(type: "integer", nullable: true),
                    mensagem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    link = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    lida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_hora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacoes", x => x.notificacao_id);
                    table.ForeignKey(
                        name: "fk_notificacoes_agendamentos_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamentos",
                        principalColumn: "agendamento_id");
                    table.ForeignKey(
                        name: "fk_notificacoes_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notificacoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    pagamento_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agendamento_id = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<int>(type: "integer", nullable: true),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status_pagamento = table.Column<int>(type: "integer", nullable: false),
                    payment_id = table.Column<string>(type: "text", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    barbearia_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamentos", x => x.pagamento_id);
                    table.ForeignKey(
                        name: "fk_pagamentos_agendamentos_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamentos",
                        principalColumn: "agendamento_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pagamentos_barbearias_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbearias",
                        principalColumn: "barbearia_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamentos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "cliente_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_servicos_servico_id",
                table: "agendamento_servicos",
                column: "servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamentos_barbearia_id",
                table: "agendamentos",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamentos_barbeiro_id",
                table: "agendamentos",
                column: "barbeiro_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamentos_cliente_id",
                table: "agendamentos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_avaliacao_agendamento_id",
                table: "avaliacao",
                column: "agendamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_barbearias_url_slug",
                table: "barbearias",
                column: "url_slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_barbeiro_servicos_servico_id",
                table: "barbeiro_servicos",
                column: "servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_barbeiros_barbearia_id",
                table: "barbeiros",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_barbearia_id",
                table: "clientes",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_feriados_barbearias_barbearia_id",
                table: "feriados_barbearias",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_indisponibilidades_barbeiros_barbeiro_id",
                table: "indisponibilidades_barbeiros",
                column: "barbeiro_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_agendamento_id",
                table: "notificacoes",
                column: "agendamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_barbearia_id",
                table: "notificacoes",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_id",
                table: "notificacoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_agendamento_id",
                table: "pagamentos",
                column: "agendamento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_barbearia_id",
                table: "pagamentos",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_cliente_id",
                table: "pagamentos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_assinatura_barbearias_barbearia_id",
                table: "plano_assinatura_barbearias",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_beneficios_plano_barbearia_id",
                table: "plano_beneficios",
                column: "plano_barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_beneficios_servico_id",
                table: "plano_beneficios",
                column: "servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_personalizados_usuario_id",
                table: "relatorios_personalizados",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_servicos_barbearia_id",
                table: "servicos",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_barbearia_id",
                table: "usuarios",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agendamento_servicos");

            migrationBuilder.DropTable(
                name: "avaliacao");

            migrationBuilder.DropTable(
                name: "barbeiro_servicos");

            migrationBuilder.DropTable(
                name: "feriados_barbearias");

            migrationBuilder.DropTable(
                name: "feriados_nacionais");

            migrationBuilder.DropTable(
                name: "grafico_posicao");

            migrationBuilder.DropTable(
                name: "indisponibilidades_barbeiros");

            migrationBuilder.DropTable(
                name: "logs");

            migrationBuilder.DropTable(
                name: "notificacoes");

            migrationBuilder.DropTable(
                name: "pagamentos");

            migrationBuilder.DropTable(
                name: "pagamentos_assinaturas_site");

            migrationBuilder.DropTable(
                name: "plano_assinatura_sistema");

            migrationBuilder.DropTable(
                name: "plano_beneficios");

            migrationBuilder.DropTable(
                name: "push_subscriptions");

            migrationBuilder.DropTable(
                name: "relatorios_personalizados");

            migrationBuilder.DropTable(
                name: "agendamentos");

            migrationBuilder.DropTable(
                name: "plano_assinatura_barbearias");

            migrationBuilder.DropTable(
                name: "servicos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "barbeiros");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "barbearias");
        }
    }
}
