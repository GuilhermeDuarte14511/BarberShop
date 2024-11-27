using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BarberShop.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var barbeariaUrl = context.Session.GetString("BarbeariaUrl");
                var currentPath = context.Request.Path.Value.ToLower();

                if (!string.IsNullOrEmpty(barbeariaUrl))
                {
                    if (currentPath == $"/{barbeariaUrl}".ToLower())
                    {
                        context.Response.Redirect($"/{barbeariaUrl}/Cliente/MenuPrincipal");
                        return;
                    }

                    // Redirecionar admins e barbeiros para a página administrativa
                    if (currentPath == $"/{barbeariaUrl}/admin".ToLower() &&
                        (context.User.IsInRole("Admin") || context.User.IsInRole("Barbeiro")))
                    {
                        context.Response.Redirect($"/{barbeariaUrl}/Admin/Index");
                        return;
                    }
                }
            }

            await _next(context);
        }

    }
}
