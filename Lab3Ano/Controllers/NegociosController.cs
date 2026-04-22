using Lab3Ano.Data;
using Lab3Ano.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab3Ano.Controllers
{
    [Authorize]
    public class NegociosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NegociosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Comprador> GetCompradorAtualAsync()
        {
            var userId = _userManager.GetUserId(User);
            var comprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == userId);

            if (comprador == null)
            {
                var user = await _userManager.GetUserAsync(User);
                comprador = new Comprador
                {
                    UserId = userId,
                    Nome = user.UserName.Split('@')[0],
                    Contacto = user.PhoneNumber,
                    Morada = "N/A",
                    Localidade = "N/A",
                    CodPostal = "0000",
                    Pais = "Portugal"
                };
                _context.Comprador.Add(comprador);
                await _context.SaveChangesAsync();
            }
            return comprador;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(int anuncioId)
        {
            var comprador = await GetCompradorAtualAsync();
            var anuncio = await _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null || anuncio.Estado != "Ativo")
            {
                TempData["Erro"] = "Este carro não está disponível para pedidos de reserva.";
                return RedirectToAction("Details", "Anuncio", new { id = anuncioId });
            }

            var existePedido = await _context.Reserva
                .AnyAsync(r => r.IdAnuncio == anuncioId && r.IdComprador == comprador.Id);

            if (existePedido)
            {
                TempData["Info"] = "Já tens um pedido de reserva para este carro.";
                return RedirectToAction("Details", "Anuncio", new { id = anuncioId });
            }

            var reserva = new Reserva
            {
                IdAnuncio = anuncioId,
                IdComprador = comprador.Id,
                DataReserva = DateTime.Now,
                DataExpira = DateTime.Now.AddDays(3),
                IsAtiva = false
            };
            _context.Reserva.Add(reserva);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            string destinatarioId = anuncio.IdVendedorNavigation.UserId;

            var mensagemAuto = new Mensagem
            {
                RemetenteId = user.Id,
                DestinatarioId = destinatarioId,
                AnuncioId = anuncioId,
                Conteudo = "Olá! Fiz um pedido de reserva para este veículo. Aguardo a tua aprovação.",
                DataEnvio = DateTime.UtcNow,
                Lida = false
            };
            _context.Mensagem.Add(mensagemAuto);

            var notif = new Notificacao
            {
                UserId = destinatarioId,
                Texto = $"Novo pedido de reserva no anúncio #{anuncioId}",
                UrlDestino = $"/Mensagens/Chat?outroUserId={user.Id}&anuncioId={anuncioId}",
                Data = DateTime.UtcNow,
                Lida = false
            };
            _context.Notificacao.Add(notif);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Pedido de reserva enviado! Verifica o chat.";
            return RedirectToAction("Chat", "Mensagens", new { outroUserId = destinatarioId, anuncioId = anuncioId });
        }

        [HttpPost]
        public async Task<IActionResult> AprovarReserva(int reservaId)
        {
            var userId = _userManager.GetUserId(User);
            var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendedor == null) return Unauthorized();

            var reserva = await _context.Reserva
                .Include(r => r.IdAnuncioNavigation)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva == null || reserva.IdAnuncioNavigation.IdVendedor != vendedor.Id)
            {
                TempData["Erro"] = "Reserva não encontrada.";
                return Redirect("/Identity/Account/Manage");
            }

            if (reserva.IdAnuncioNavigation.Estado == "Vendido")
            {
                TempData["Erro"] = "Erro: Este carro já foi vendido. O pedido será removido.";
                _context.Reserva.Remove(reserva);
                await _context.SaveChangesAsync();
                return Redirect("/Identity/Account/Manage");
            }

            reserva.IsAtiva = true;
            reserva.DataExpira = DateTime.Now.AddDays(3);
            reserva.IdAnuncioNavigation.Estado = "Reservado";
            _context.Update(reserva.IdAnuncioNavigation);
            _context.Update(reserva);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Reserva aprovada! O carro está agora reservado.";
            return Redirect("/Identity/Account/Manage");
        }

        [HttpPost]
        public async Task<IActionResult> RejeitarReserva(int reservaId)
        {
            var userId = _userManager.GetUserId(User);
            var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == userId);

            var reserva = await _context.Reserva
                .Include(r => r.IdAnuncioNavigation)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva != null && reserva.IdAnuncioNavigation.IdVendedor == vendedor.Id)
            {
                _context.Reserva.Remove(reserva);

                if (reserva.IsAtiva == true && reserva.IdAnuncioNavigation.Estado == "Reservado")
                {
                    reserva.IdAnuncioNavigation.Estado = "Ativo";
                }

                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Pedido rejeitado/cancelado.";
            }

            return Redirect("/Identity/Account/Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comprar(int anuncioId)
        {
            var comprador = await GetCompradorAtualAsync();
            var anuncio = await _context.Anuncio
                .Include(a => a.IdCarroNavigation)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null || anuncio.Estado == "Vendido")
            {
                TempData["Erro"] = "Este carro já não está disponível.";
                return RedirectToAction("Details", "Anuncio", new { id = anuncioId });
            }

            var novaCompra = new Compra
            {
                IdAnuncio = anuncioId,
                IdComprador = comprador.Id,
                DataCompra = DateTime.Now,
                Valor = anuncio.IdCarroNavigation.Preco,
                EstadoPagamento = "Pago"
            };
            _context.Compra.Add(novaCompra);

            anuncio.Estado = "Vendido";
            _context.Update(anuncio);

            var todasReservas = await _context.Reserva
                .Where(r => r.IdAnuncio == anuncioId)
                .ToListAsync();

            foreach (var r in todasReservas)
            {
                r.IsAtiva = false;
                _context.Update(r);
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Compra realizada com sucesso! O carro é teu.";
            return Redirect("/Identity/Account/Manage?tab=pagamentos");
        }

        [HttpPost]
        public async Task<IActionResult> AgendarVisita(int anuncioId, DateTime dataHora)
        {
            var comprador = await GetCompradorAtualAsync();
            var anuncio = await _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null) return NotFound();

            var visita = new Visita
            {
                IdAnuncio = anuncioId,
                IdComprador = comprador.Id,
                DataHora = dataHora,
                Estado = "Pendente"
            };
            _context.Visita.Add(visita);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            string destinatarioId = anuncio.IdVendedorNavigation.UserId;

            var mensagemAuto = new Mensagem
            {
                RemetenteId = user.Id,
                DestinatarioId = destinatarioId,
                AnuncioId = anuncioId,
                Conteudo = $"Olá! Gostaria de agendar uma visita para dia {dataHora:dd/MM/yyyy às HH:mm}. Aguardo confirmação.",
                DataEnvio = DateTime.UtcNow,
                Lida = false
            };
            _context.Mensagem.Add(mensagemAuto);

            var notif = new Notificacao
            {
                UserId = destinatarioId,
                Texto = $"Novo pedido de visita no anúncio #{anuncioId}",
                UrlDestino = $"/Mensagens/Chat?outroUserId={user.Id}&anuncioId={anuncioId}",
                Data = DateTime.UtcNow,
                Lida = false
            };
            _context.Notificacao.Add(notif);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Visita solicitada! O vendedor irá confirmar no chat.";
            return RedirectToAction("Chat", "Mensagens", new { outroUserId = destinatarioId, anuncioId = anuncioId });
        }
    }
}