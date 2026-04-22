using Lab3Ano.Data;
using Lab3Ano.Helpers;
using Lab3Ano.Models;
using Lab3Ano.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Lab3Ano.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleSeguirMarca(int idMarca)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existente = await _context.MarcaFavorita
                .FirstOrDefaultAsync(mf => mf.UserId == userId && mf.MarcaId == idMarca);

            bool passouASeguir;

            if (existente != null)
            {
                _context.MarcaFavorita.Remove(existente);
                passouASeguir = false;
            }
            else
            {
                var novo = new MarcaFavorita { UserId = userId, MarcaId = idMarca };
                _context.MarcaFavorita.Add(novo);
                passouASeguir = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isFollowing = passouASeguir });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int carId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorito = await _context.Favorito
                .FirstOrDefaultAsync(f => f.IdAnuncio == carId && f.IdComprador == userId);

            bool isLiked;

            if (favorito != null)
            {
                _context.Favorito.Remove(favorito);
                isLiked = false;
            }
            else
            {
                var novo = new Favorito { IdAnuncio = carId, IdComprador = userId };
                _context.Favorito.Add(novo);
                isLiked = true;
            }

            await _context.SaveChangesAsync();

            int novaContagem = await _context.Favorito.CountAsync(f => f.IdAnuncio == carId);
            return Json(new { success = true, isLiked = isLiked, newCount = novaContagem });
        }

        [Authorize]
        public async Task<IActionResult> MyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var listaFavoritos = await _context.Favorito
                .Include(f => f.IdAnuncioNavigation)
                    .ThenInclude(a => a.IdVendedorNavigation)
                .Include(f => f.IdAnuncioNavigation)
                    .ThenInclude(a => a.IdCarroNavigation)
                        .ThenInclude(c => c.IdModeloNavigation)
                            .ThenInclude(m => m.IdMarcaNavigation)
                .Include(f => f.IdAnuncioNavigation)
                    .ThenInclude(a => a.Imagems)
                .Where(f => f.IdComprador == userId)
                .Where(f => f.IdAnuncioNavigation.Estado == "Ativo" ||
                            f.IdAnuncioNavigation.Estado == "ativo" ||
                            f.IdAnuncioNavigation.Estado == null)
                .Where(f => f.IdAnuncioNavigation.IdVendedorNavigation.IsBloqueado == false)
                .ToListAsync();

            return View(listaFavoritos);
        }

        public async Task<IActionResult> Index(string searchString, string marca, string modelo, int? precoMin, int? precoMax, string categoria, string combustivel, int? pageNumber)
        {
            var todasMarcas = await _context.Marca.OrderBy(m => m.Nome).ToListAsync();
            ViewBag.TodasMarcas = todasMarcas;

            var marcasSeguidas = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                marcasSeguidas = await _context.MarcaFavorita
                    .Where(mf => mf.UserId == userId)
                    .Select(mf => mf.MarcaId)
                    .ToListAsync();
            }
            ViewBag.MarcasSeguidas = marcasSeguidas;

            bool existemCarrosNaBD = await _context.Anuncio
                .AnyAsync(a => a.Estado == "Ativo" && !a.IdVendedorNavigation.IsBloqueado);
            ViewBag.HaCarros = existemCarrosNaBD;

            ViewBag.Marcas = new SelectList(_context.Marca.OrderBy(m => m.Nome), "Nome", "Nome", marca);
            ViewBag.Modelos = new SelectList(new List<string>());
            ViewBag.Categorias = new SelectList(_context.Categoria.OrderBy(c => c.Nome), "Nome", "Nome", categoria);
            ViewBag.Combustiveis = new SelectList(_context.Combustivel.OrderBy(c => c.Tipo), "Tipo", "Tipo", combustivel);

            ViewData["CurrentFilter"] = searchString;
            ViewData["Marca"] = marca;
            ViewData["Modelo"] = modelo;
            ViewData["PrecoMin"] = precoMin;
            ViewData["PrecoMax"] = precoMax;
            ViewData["Categoria"] = categoria;
            ViewData["Combustivel"] = combustivel;

            var query = _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdModeloNavigation)
                        .ThenInclude(m => m.IdMarcaNavigation)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdCombustivelNavigation)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdCategoriaNavigation)
                .Include(a => a.Imagems)
                .AsNoTracking();

            query = query.Where(a => a.IdVendedorNavigation.IsBloqueado == false);
            query = query.Where(a => a.Estado == "Ativo" || a.Estado == "ativo" || a.Estado == null);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Titulo.Contains(searchString) ||
                                         a.IdCarroNavigation.IdModeloNavigation.IdMarcaNavigation.Nome.Contains(searchString) ||
                                         a.IdCarroNavigation.IdModeloNavigation.Nome.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(marca)) query = query.Where(a => a.IdCarroNavigation.IdModeloNavigation.IdMarcaNavigation.Nome == marca);
            if (!string.IsNullOrEmpty(modelo)) query = query.Where(a => a.IdCarroNavigation.IdModeloNavigation.Nome == modelo);
            if (!string.IsNullOrEmpty(categoria)) query = query.Where(a => a.IdCarroNavigation.IdCategoriaNavigation.Nome == categoria);
            if (!string.IsNullOrEmpty(combustivel)) query = query.Where(a => a.IdCarroNavigation.IdCombustivelNavigation.Tipo == combustivel);
            if (precoMin.HasValue) query = query.Where(a => a.IdCarroNavigation.Preco >= precoMin.Value);
            if (precoMax.HasValue) query = query.Where(a => a.IdCarroNavigation.Preco <= precoMax.Value);

            query = query.OrderByDescending(a => a.DataCriacao);

            int tamanhoPagina = 9;

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.UserLikes = await _context.Favorito
                    .Where(f => f.IdComprador == userId)
                    .Select(f => f.IdAnuncio)
                    .ToListAsync();
            }
            else
            {
                ViewBag.UserLikes = new List<int>();
            }

            return View(await PaginatedList<Lab3Ano.Models.Entidades.Anuncio>.CreateAsync(query, pageNumber ?? 1, tamanhoPagina));
        }

        [HttpGet]
        public JsonResult GetModelosByMarca(string nomeMarca)
        {
            var modelos = _context.Modelo
                .Include(m => m.IdMarcaNavigation)
                .Where(m => m.IdMarcaNavigation.Nome == nomeMarca)
                .OrderBy(m => m.Nome)
                .Select(m => new { value = m.Nome, text = m.Nome })
                .ToList();

            return Json(modelos);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        public IActionResult Sobre()
        {
            var info = new MarketplaceInfoViewModel
            {
                Nome = "VilaMarket", 
                EmailSuporte = "StandVilaMarket@gmail.com",
                Telefone = "A Definir",
                HorarioSuporte = "Segunda a Sexta: 09:00 - 18:00 | Sábados: 10:00 - 13:00",

                TermosUso = "1. Ao utilizar este serviço, o utilizador concorda com a veracidade dos dados.\n" +
                            "2. Os anúncios são da responsabilidade dos vendedores.\n" +
                            "3. As transações financeiras são tratadas externamente.",

                PoliticaPrivacidade = "Respeitamos a sua privacidade. Os dados recolhidos (email, contacto) " +
                                      "são utilizados apenas para fins de contacto entre comprador e vendedor " +
                                      "e não são partilhados com terceiros sem consentimento."
            };

            return View(info);
        }
    }
}