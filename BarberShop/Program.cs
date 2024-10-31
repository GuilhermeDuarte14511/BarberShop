using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using MercadoPago.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Stripe;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configuração da conexão com o banco de dados usando appsettings.json
builder.Services.AddDbContext<BarbeariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarberShopDb")));

// Carregar secrets somente em ambiente de desenvolvimento
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configurar a chave de acesso do Mercado Pago com base no ambiente
string mercadoPagoAccessToken = builder.Environment.IsDevelopment()
    ? "TEST-236275902252089-103018-3e74429f349b5f03d14f49344da52ec9-430792758" // Chave de teste
    : Environment.GetEnvironmentVariable("APP_USR-236275902252089-103018-82740cb4879d6a2e847fc29f38854d56-430792758"); // Variável de ambiente para chave de produção

if (string.IsNullOrEmpty(mercadoPagoAccessToken))
{
    Console.WriteLine("Aviso: Token do Mercado Pago não configurado. Verifique se a variável de ambiente 'MERCADO_PAGO_PRODUCAO' está definida.");
}
else
{
    MercadoPagoConfig.AccessToken = mercadoPagoAccessToken;
}

// Configurar o Stripe com as configurações de appsettings.json
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registrar serviços de aplicação e infraestrutura
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IBarbeiroRepository, BarbeiroRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IRepository<AgendamentoServico>, AgendamentoServicoRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<AutenticacaoService>();

// Configurar o SendGrid API Key dinamicamente com base no ambiente
string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]
    : Environment.GetEnvironmentVariable("SendGridApiKey");

builder.Services.AddScoped<IEmailService, EmailService>(provider =>
    new EmailService(sendGridApiKey));

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Adicionar serviços MVC e configurar o Swagger
builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BarberShop API",
        Version = "v1",
        Description = "API para gerenciamento de agendamentos, pagamentos e serviços da BarberShop."
    });

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

// Configuração de middlewares de tratamento de erro
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

    var requestLog = new Log
    {
        LogLevel = "INFO",
        Source = "Middleware",
        Message = $"Requisição recebida - Método: {request.Method}, Path: {request.Path}, Query: {request.QueryString}",
        Data = string.Join(", ", request.Headers.Select(h => $"{h.Key}: {h.Value}")),
        LogDateTime = DateTime.UtcNow
    };

    dbContext.Logs.Add(requestLog);
    await dbContext.SaveChangesAsync();

    await next();

    var response = context.Response;
    var responseLog = new Log
    {
        LogLevel = "INFO",
        Source = "Middleware",
        Message = $"Resposta enviada - Status Code: {response.StatusCode}",
        Data = null,
        LogDateTime = DateTime.UtcNow
    };

    dbContext.Logs.Add(responseLog);
    await dbContext.SaveChangesAsync();
});

// Habilitar Swagger e configurar HTTPS e arquivos estáticos
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

// Mapeamento de controladores e rotas
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
