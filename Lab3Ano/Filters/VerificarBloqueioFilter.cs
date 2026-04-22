using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Lab3Ano.Data; // Para aceder ao ApplicationDbContext

namespace Lab3Ano.Filters
{
    public class VerificarBloqueioFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public VerificarBloqueioFilter(ApplicationDbContext context, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(context.HttpContext.User);

                if (!string.IsNullOrEmpty(userId))
                {
                    var vendedorBloqueado = await _context.Vendedor
                        .AnyAsync(v => v.UserId == userId && v.IsBloqueado);

                    var compradorBloqueado = await _context.Comprador
                        .AnyAsync(c => c.UserId == userId && c.IsBloqueado);

                    if (vendedorBloqueado || compradorBloqueado)
                    {
                        await _signInManager.SignOutAsync();
                        context.Result = new RedirectToPageResult("/Account/Login", new { area = "Identity", erro = "bloqueado" });
                        return;
                    }
                }
            }

            await next();
        }
    }
}