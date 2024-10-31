using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Configurar a string de conexão usando o appsettings.json
builder.Services.AddDbContext<BarbeariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarberShopDb")));

// Carregar secrets somente em Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configurar o token de acesso do Mercado Pago em appsettings.json ou como variável de ambiente
string mercadoPagoAccessToken = builder.Configuration["MercadoPago:AccessToken"]
    ?? Environment.GetEnvironmentVariable("MercadoPagoAccessToken");

// Configurar o Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registrar o serviço de pagamento com Stripe
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Obter a chave SendGridApiKey dinamicamente com base no ambiente
string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]
    : Environment.GetEnvironmentVariable("SendGridApiKey");

// Configurar o serviço de Email com SendGrid usando a chave configurada
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
    new EmailService(sendGridApiKey));

// Registrar repositórios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IBarbeiroRepository, BarbeiroRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IRepository<AgendamentoServico>, AgendamentoServicoRepository>();

// Registrar serviços da camada de aplicação
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Registrar o AutenticacaoService
builder.Services.AddScoped<AutenticacaoService>();

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Adicionar serviços MVC
builder.Services.AddControllersWithViews();

// Configurar Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BarberShop API",
        Version = "v1",
        Description = "API para gerenciamento de agendamentos, pagamentos e serviços da BarberShop."
    });

    // Configurar autenticação com Bearer token no Swagger, se necessário
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no campo abaixo (ex: 'Bearer {token}')"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configurar pipeline de processamento de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware para registrar e salvar cada requisição HTTP no banco de dados
app.Use(async (context, next) =>
{
    var request = context.Request;
    var dbContext = context.RequestServices.GetRequiredService<BarbeariaContext>();

    // Preparar informações da requisição para salvar no log
    var requestLog = new Log
    {
        LogLevel = "INFO",
        Source = "Middleware",
        Message = $"Requisição recebida - Método: {request.Method}, Path: {request.Path}, Query: {request.QueryString}",
        Data = string.Join(", ", request.Headers.Select(h => $"{h.Key}: {h.Value}")),
        LogDateTime = DateTime.UtcNow
    };

    // Salvar log da requisição no banco de dados
    dbContext.Logs.Add(requestLog);
    await dbContext.SaveChangesAsync();

    // Executar o próximo middleware no pipeline
    await next();

    // Preparar informações da resposta para salvar no log
    var response = context.Response;
    var responseLog = new Log
    {
        LogLevel = "INFO",
        Source = "Middleware",
        Message = $"Resposta enviada - Status Code: {response.StatusCode}",
        Data = null,
        LogDateTime = DateTime.UtcNow
    };

    // Salvar log da resposta no banco de dados
    dbContext.Logs.Add(responseLog);
    await dbContext.SaveChangesAsync();
});

// Habilitar o middleware do Swagger para todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarberShop API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Mapeia os controladores que possuem rotas definidas por atributos, como o WebhookController
app.MapControllers(); // <- ESSA LINHA É NECESSÁRIA

// Configuração de rota padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
