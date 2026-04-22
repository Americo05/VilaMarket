using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab3Ano.Data;
using Lab3Ano.Models.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Lab3Ano.Controllers
{
    [Authorize] // Só funciona se estiver logado
    public class NotificacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificacoesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Devolve o NÚMERO de notificações não lidas (para a bolinha vermelha)
        [HttpGet]
        public async Task<IActionResult> ObterContagem()
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.Notificacao.CountAsync(n => n.UserId == userId && !n.Lida);
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> ObterUltimas()
        {
            var userId = _userManager.GetUserId(User);

            var notifs = await _context.Notificacao
                .Where(n => n.UserId == userId) 
                .OrderByDescending(n => n.Data)
                .Take(7)
                .Select(n => new {
                    n.Id,
                    n.Texto,
                    n.UrlDestino,
                    Data = n.Data.ToString("dd/MM HH:mm"),
                    n.Lida
                })
                .ToListAsync();

            return Json(notifs);
        }

        // 3. Marca uma notificação como LIDA quando clicas nela
        [HttpPost]
        public async Task<IActionResult> MarcarComoLida(int id)
        {
            var userId = _userManager.GetUserId(User);
            var notif = await _context.Notificacao.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notif != null)
            {
                notif.Lida = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> LimparTodas()
        {
            var userId = _userManager.GetUserId(User);

            var lista = await _context.Notificacao
                .Where(n => n.UserId == userId && n.Lida)
                .ToListAsync();

            if (lista.Any())
            {
                _context.Notificacao.RemoveRange(lista);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}