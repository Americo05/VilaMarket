using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab3Ano.Data;
using Lab3Ano.Models.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Lab3Ano.Controllers
{
    [Authorize]
    public class MensagensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MensagensController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var meuId = _userManager.GetUserId(User);

            var mensagens = await _context.Mensagem
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .Include(m => m.Anuncio)
                .ThenInclude(a => a.IdCarroNavigation)
                .ThenInclude(c => c.IdModeloNavigation)
                .ThenInclude(m => m.IdMarcaNavigation)
                .Where(m => m.RemetenteId == meuId || m.DestinatarioId == meuId)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            var conversas = mensagens
                .GroupBy(m => new
                {
                    OutroUserId = m.RemetenteId == meuId ? m.DestinatarioId : m.RemetenteId,
                    AnuncioId = m.AnuncioId
                })
                .Select(g => g.First())
                .ToList();

            return View(conversas);
        }

        public async Task<IActionResult> Chat(string outroUserId, int? anuncioId)
        {
            if (string.IsNullOrEmpty(outroUserId)) return NotFound();

            var meuId = _userManager.GetUserId(User);

            // Carrega as ViewBags necessárias
            await CarregarDadosChat(outroUserId, anuncioId, meuId);

            bool houveAlteracoes = false;

            var mensagensNaoLidas = await _context.Mensagem
                .Where(m => m.RemetenteId == outroUserId &&
                            m.DestinatarioId == meuId &&
                            m.AnuncioId == anuncioId &&
                            !m.Lida)
                .ToListAsync();

            if (mensagensNaoLidas.Any())
            {
                foreach (var msg in mensagensNaoLidas) msg.Lida = true;
                houveAlteracoes = true;
            }

            var notificacoesPendentes = await _context.Notificacao
                .Where(n => n.UserId == meuId && !n.Lida &&
                            n.UrlDestino.Contains(outroUserId) &&
                            (anuncioId == null || n.UrlDestino.Contains($"anuncioId={anuncioId}")))
                .ToListAsync();

            if (notificacoesPendentes.Any())
            {
                foreach (var notif in notificacoesPendentes) notif.Lida = true;
                houveAlteracoes = true;
            }

            if (houveAlteracoes) await _context.SaveChangesAsync();

            var historico = await _context.Mensagem
                .Where(m => ((m.RemetenteId == meuId && m.DestinatarioId == outroUserId) ||
                             (m.RemetenteId == outroUserId && m.DestinatarioId == meuId))
                             && m.AnuncioId == anuncioId)
                .OrderBy(m => m.Id)
                .ToListAsync();

            var outroUser = await _userManager.FindByIdAsync(outroUserId);

            ViewBag.OutroUserId = outroUserId;
            ViewBag.OutroUserName = outroUser?.UserName ?? "Utilizador";
            ViewBag.MeuId = meuId;
            ViewBag.AnuncioId = anuncioId;

            return View(historico);
        }

        // MÉTODO AUXILIAR PARA NÃO REPETIR CÓDIGO E GARANTIR QUE AS VIEW_BAGS EXISTEM
        private async Task CarregarDadosChat(string outroUserId, int? anuncioId, string meuId)
        {
            var anuncio = await _context.Anuncio.FirstOrDefaultAsync(a => a.Id == anuncioId);
            bool souEuOVendedor = false;

            if (anuncio != null)
            {
                if (anuncio.IdVendedor.ToString() == meuId)
                {
                    souEuOVendedor = true;
                }
                else
                {
                    var meuRegistoVendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == meuId);
                    if (meuRegistoVendedor != null && anuncio.IdVendedor.ToString() == meuRegistoVendedor.Id.ToString())
                    {
                        souEuOVendedor = true;
                    }
                }
            }

            ViewBag.SouVendedor = souEuOVendedor;

            string idUserComprador = souEuOVendedor ? outroUserId : meuId;
            var compradorRegisto = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == idUserComprador);

            if (compradorRegisto != null && anuncioId.HasValue)
            {
                ViewBag.VisitaPendente = await _context.Visita
                    .FirstOrDefaultAsync(v => v.IdAnuncio == anuncioId &&
                                              v.IdComprador == compradorRegisto.Id &&
                                              v.Estado == "Pendente");

                ViewBag.ReservaPendente = await _context.Reserva
                    .FirstOrDefaultAsync(r => r.IdAnuncio == anuncioId &&
                                              r.IdComprador == compradorRegisto.Id &&
                                              r.IsAtiva == false);
            }
        }

        public async Task<IActionResult> ChatPartial(string outroUserId, int? anuncioId)
        {
            if (string.IsNullOrEmpty(outroUserId)) return NotFound();
            var meuId = _userManager.GetUserId(User);

            // IMPORTANTE: Recarregar as ViewBags aqui também!
            await CarregarDadosChat(outroUserId, anuncioId, meuId);

            var historico = await _context.Mensagem
                .Where(m => ((m.RemetenteId == meuId && m.DestinatarioId == outroUserId) ||
                             (m.RemetenteId == outroUserId && m.DestinatarioId == meuId))
                             && m.AnuncioId == anuncioId)
                .OrderBy(m => m.Id)
                .ToListAsync();

            var outroUser = await _userManager.FindByIdAsync(outroUserId);
            ViewBag.OutroUserId = outroUserId;
            ViewBag.OutroUserName = outroUser?.UserName ?? "Utilizador";
            ViewBag.MeuId = meuId;
            ViewBag.AnuncioId = anuncioId;

            return PartialView("_ChatPartial", historico);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarVisita(int visitaId, string outroUserId, int anuncioId)
        {
            var visita = await _context.Visita.FindAsync(visitaId);
            if (visita != null)
            {
                visita.Estado = "Confirmada";
                await _context.SaveChangesAsync();
                await Enviar(outroUserId, "A visita foi confirmada! Vemo-nos lá.", anuncioId);
            }
            return RedirectToAction("Chat", new { outroUserId, anuncioId });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarReserva(int reservaId, string outroUserId, int anuncioId)
        {
            var reserva = await _context.Reserva.FindAsync(reservaId);
            if (reserva != null)
            {
                reserva.IsAtiva = true;
                await _context.SaveChangesAsync();
                await Enviar(outroUserId, "A reserva foi aceite. O carro está reservado para ti.", anuncioId);
            }
            return RedirectToAction("Chat", new { outroUserId, anuncioId });
        }

        [HttpPost]
        public async Task<IActionResult> Enviar(string destinatarioId, string conteudo, int? anuncioId)
        {
            if (string.IsNullOrWhiteSpace(conteudo) || string.IsNullOrEmpty(destinatarioId))
            {
                return Json(new { success = false, message = "Dados inválidos." });
            }

            var meuId = _userManager.GetUserId(User);

            var novaMsg = new Mensagem
            {
                RemetenteId = meuId,
                DestinatarioId = destinatarioId,
                Conteudo = conteudo,
                DataEnvio = DateTime.UtcNow,
                AnuncioId = anuncioId,
                Lida = false
            };

            try
            {
                _context.Mensagem.Add(novaMsg);
                await _context.SaveChangesAsync();

                var notificacaoMsg = new Notificacao
                {
                    UserId = destinatarioId,
                    Texto = $"Nova mensagem sobre o anúncio #{anuncioId}",
                    UrlDestino = $"/Mensagens/Chat?outroUserId={meuId}&anuncioId={anuncioId}",
                    Data = DateTime.UtcNow,
                    Lida = false
                };

                _context.Notificacao.Add(notificacaoMsg);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}