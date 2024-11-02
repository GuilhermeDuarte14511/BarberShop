using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
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

// Configurar o Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registrar o serviço de pagamento com Stripe
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Registrar o serviço de logging customizado
builder.Services.AddScoped<ILogService, LogService>();

// Obter a chave SendGridApiKey dinamicamente com base no ambiente
string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]  // Obtém dos secrets em Development
    : Environment.GetEnvironmentVariable("SendGridApiKey"); // Obtém da variável de ambiente na Azure

// Configurar o serviço de Email com SendGrid usando a chave configurada
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
{
    var logService = provider.GetRequiredService<ILogService>(); // Obtém a instância de ILogService
    return new EmailService(sendGridApiKey, logService); // Passa sendGridApiKey e logService para o construtor
});
// Obter a PublishableKey do Stripe e definir para a ViewData na aplicação
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return configuration["Stripe:PublishableKey"];
});

// Serviço RabbitMQ (Comentado porque você não irá usar agora)
/*
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>(provider =>
    new RabbitMQService(builder.Configuration["SendGridApiKey"], provider));
*/

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

// Registrar o AutenticacaoService
builder.Services.AddScoped<AutenticacaoService>();

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";   // Define o caminho da página de login
        options.LogoutPath = "/Login/Logout"; // Define o caminho da página de logout
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tempo de expiração da sessão (opcional)
        options.SlidingExpiration = true; // Expiração deslizante (opcional)
    });

// Adicionar serviços MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar pipeline de processamento de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

// Inicializa o consumidor RabbitMQ ao iniciar o aplicativo (Comentado)
/*
var rabbitMQService = app.Services.GetRequiredService<IRabbitMQService>();
Task.Run(() => rabbitMQService.IniciarConsumo());
*/

app.Run();
