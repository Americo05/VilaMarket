using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab3Ano.Data;
using Lab3Ano.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Lab3Ano.Models.Entidades;
using System.Security.Claims;
using System.Linq;

namespace Lab3Ano.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var todosUsers = await _userManager.Users.ToListAsync();

            int contaAdmins = 0;
            int contaVendedores = 0;
            int contaCompradores = 0;

            foreach (var user in todosUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    contaAdmins++;
                }
                else if (await _userManager.IsInRoleAsync(user, "Vendedor") ||
                    _context.Vendedor.Any(v => v.UserId == user.Id && v.IsAprovado == true))
                {
                    contaVendedores++;
                }
                else
                {
                    contaCompradores++;
                }
            }

            var todosAnuncios = await _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdModeloNavigation)
                        .ThenInclude(m => m.IdMarcaNavigation)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdCombustivelNavigation)
                .ToListAsync();

            int numAnunciosTotal = todosAnuncios.Count;
            int numAtivos = todosAnuncios.Count(a => a.Estado == "Ativo");
            int numReservados = todosAnuncios.Count(a => a.Estado == "Reservado");
            int numVendidos = todosAnuncios.Count(a => a.Estado == "Vendido");
            int numRemovidos = todosAnuncios.Count(a => a.Estado == "Removido" || a.Estado == "Pausado");

            int numPendentes = await _context.Vendedor.CountAsync(v => v.IsAprovado == false);
            ViewBag.PedidosPendentes = numPendentes;
            ViewBag.TotalAdmins = contaAdmins;

            ViewBag.AnunciosAtivos = numAtivos;
            ViewBag.AnunciosReservados = numReservados;
            ViewBag.AnunciosVendidos = numVendidos;
            ViewBag.AnunciosRemovidos = numRemovidos;

            decimal valorTotalStock = todosAnuncios.Where(a => a.Estado == "Ativo").Sum(a => a.IdCarroNavigation.Preco);
            ViewBag.ValorTotalStock = valorTotalStock;

            decimal precoMedio = todosAnuncios.Any() ? todosAnuncios.Average(a => a.IdCarroNavigation.Preco) : 0;
            ViewBag.PrecoMedio = precoMedio;

            var topMarcas = todosAnuncios
                .GroupBy(a => a.IdCarroNavigation.IdModeloNavigation.IdMarcaNavigation.Nome)
                .Select(g => new { Marca = g.Key, Quantidade = g.Count() })
                .OrderByDescending(x => x.Quantidade)
                .Take(5)
                .ToList();
            ViewBag.TopMarcasLabels = topMarcas.Select(x => x.Marca).ToArray();
            ViewBag.TopMarcasData = topMarcas.Select(x => x.Quantidade).ToArray();

            var combustiveis = todosAnuncios
                .GroupBy(a => a.IdCarroNavigation.IdCombustivelNavigation.Tipo)
                .Select(g => new { Tipo = g.Key, Quantidade = g.Count() })
                .ToList();
            ViewBag.CombustivelLabels = combustiveis.Select(x => x.Tipo).ToArray();
            ViewBag.CombustivelData = combustiveis.Select(x => x.Quantidade).ToArray();

            var topLocais = todosAnuncios
                .GroupBy(a => a.IdCarroNavigation.Localizacao)
                .Select(g => new { Cidade = g.Key, Quantidade = g.Count() })
                .OrderByDescending(x => x.Quantidade)
                .Take(5)
                .ToList();
            ViewBag.TopLocais = topLocais;

            var model = new AdminDashboardViewModel
            {
                TotalUtilizadores = todosUsers.Count,
                TotalCompradores = contaCompradores,
                TotalVendedores = contaVendedores,
                TotalAnuncios = numAnunciosTotal
            };

            return View(model);
        }

        public async Task<IActionResult> GerirUtilizadores(string pesquisa, string filtroTipo, string sortOrder)
        {
            ViewData["PesquisaAtual"] = pesquisa;
            ViewData["FiltroTipoAtual"] = filtroTipo;
            ViewData["TipoSortParm"] = sortOrder == "Tipo" ? "tipo_desc" : "Tipo";
            ViewData["NomeSortParm"] = sortOrder == "Nome" ? "nome_desc" : "Nome";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["EstadoSortParm"] = sortOrder == "Estado" ? "estado_desc" : "Estado";

            var todosUsers = await _userManager.Users.AsNoTracking().ToListAsync();

            var vendedores = await _context.Vendedor.AsNoTracking().ToListAsync();
            var compradores = await _context.Comprador.AsNoTracking().ToListAsync();

            var listaFinal = new List<UtilizadorAdminViewModel>();

            foreach (var user in todosUsers)
            {
                var vm = new UtilizadorAdminViewModel();

                vm.Email = user.Email;
                vm.IsBloqueado = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now;

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    vm.Tipo = "Admin";
                    vm.Nome = "Admin (" + user.UserName + ")";
                    vm.IsAprovado = true;
                }
                else
                {
                    var dadosVendedor = vendedores.FirstOrDefault(v => v.UserId == user.Id);

                    if (dadosVendedor != null)
                    {
                        vm.Tipo = "Vendedor";
                        vm.Nome = dadosVendedor.Nome;
                        vm.Id = dadosVendedor.Id;
                        vm.MotivoBloqueio = dadosVendedor.MotivoBloqueio;
                        vm.IsBloqueado = dadosVendedor.IsBloqueado;
                        vm.IsAprovado = dadosVendedor.IsAprovado ?? false;
                    }
                    else
                    {
                        vm.Tipo = "Comprador";
                        var dadosComprador = compradores.FirstOrDefault(c => c.UserId == user.Id);

                        vm.Nome = dadosComprador != null ? dadosComprador.Nome : user.UserName;

                        if (dadosComprador != null)
                        {
                            vm.Id = dadosComprador.Id;
                            vm.MotivoBloqueio = dadosComprador.MotivoBloqueio;
                            vm.IsBloqueado = dadosComprador.IsBloqueado;
                        }

                        vm.IsAprovado = true;
                    }
                }

                listaFinal.Add(vm);
            }

            if (!string.IsNullOrEmpty(pesquisa))
            {
                pesquisa = pesquisa.ToLower();
                listaFinal = listaFinal.Where(u =>
                    (u.Nome != null && u.Nome.ToLower().Contains(pesquisa)) ||
                    (u.Email != null && u.Email.ToLower().Contains(pesquisa))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(filtroTipo) && filtroTipo != "Todos")
            {
                listaFinal = listaFinal.Where(u => u.Tipo == filtroTipo).ToList();
            }

            switch (sortOrder)
            {
                case "nome_desc": listaFinal = listaFinal.OrderByDescending(s => s.Nome).ToList(); break;
                case "Nome": listaFinal = listaFinal.OrderBy(s => s.Nome).ToList(); break;
                case "email_desc": listaFinal = listaFinal.OrderByDescending(s => s.Email).ToList(); break;
                case "Email": listaFinal = listaFinal.OrderBy(s => s.Email).ToList(); break;

                case "Tipo":
                    listaFinal = listaFinal.OrderBy(u =>
                        u.Tipo == "Admin" ? 1 :
                        (u.Tipo == "Vendedor" && u.IsAprovado == true) ? 2 :
                        (u.Tipo == "Vendedor" && u.IsAprovado != true) ? 3 :
                        4
                    ).ToList();
                    break;

                case "tipo_desc":
                    listaFinal = listaFinal.OrderByDescending(u =>
                        u.Tipo == "Admin" ? 1 :
                        (u.Tipo == "Vendedor" && u.IsAprovado == true) ? 2 :
                        (u.Tipo == "Vendedor" && u.IsAprovado != true) ? 3 :
                        4
                    ).ToList();
                    break;

                case "Estado":
                    listaFinal = listaFinal.OrderBy(u =>
                        u.IsBloqueado ? 1 :
                        (u.Tipo == "Vendedor" && u.IsAprovado != true) ? 2 :
                        3
                    ).ToList();
                    break;

                case "estado_desc":
                    listaFinal = listaFinal.OrderByDescending(u =>
                        u.IsBloqueado ? 1 :
                        (u.Tipo == "Vendedor" && u.IsAprovado != true) ? 2 :
                        3
                    ).ToList();
                    break;

                default:
                    listaFinal = listaFinal.OrderBy(s => s.Nome).ToList();
                    break;
            }

            return View(listaFinal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BloquearDesbloquear(int id, string tipo, string motivo)
        {
            bool deveBloquear = !string.IsNullOrWhiteSpace(motivo);

            if (tipo == "Comprador")
            {
                var comprador = await _context.Comprador.FindAsync(id);
                if (comprador != null)
                {
                    comprador.MotivoBloqueio = deveBloquear ? motivo : null;
                    comprador.IsBloqueado = deveBloquear;
                    _context.Update(comprador);
                }
            }
            else if (tipo == "Vendedor")
            {
                var vendedor = await _context.Vendedor.FindAsync(id);
                if (vendedor != null)
                {
                    vendedor.MotivoBloqueio = deveBloquear ? motivo : null;
                    vendedor.IsBloqueado = deveBloquear;
                    _context.Update(vendedor);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(GerirUtilizadores));
        }

        public async Task<IActionResult> GerirAnuncios(string pesquisa, string filtroEstado)
        {
            ViewData["PesquisaAtual"] = pesquisa;
            ViewData["FiltroEstadoAtual"] = filtroEstado;

            var query = _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .Include(a => a.IdCarroNavigation)
                .Include(a => a.Imagems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(pesquisa))
            {
                query = query.Where(a => a.Titulo.Contains(pesquisa) || a.IdVendedorNavigation.Nome.Contains(pesquisa));
            }

            if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "Todos")
            {
                query = query.Where(a => a.Estado == filtroEstado);
            }

            query = query.OrderByDescending(a => a.DataCriacao);

            var listaAnuncios = await query.Select(a => new AnuncioAdminViewModel
            {
                Id = a.Id,
                Titulo = a.Titulo,
                NomeVendedor = a.IdVendedorNavigation.Nome,
                IdVendedor = a.IdVendedor,
                Preco = a.IdCarroNavigation.Preco,
                Estado = a.Estado ?? "Ativo",
                DataCriacao = a.DataCriacao,
                ImagemCapaUrl = a.Imagems.Any() ? a.Imagems.FirstOrDefault().Url : "/img/sem-foto.jpg"
            }).ToListAsync();

            return View(listaAnuncios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarEstadoAnuncio(int id, string novoEstado)
        {
            var anuncio = await _context.Anuncio.FindAsync(id);

            if (anuncio != null)
            {
                anuncio.Estado = novoEstado;
                _context.Update(anuncio);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(GerirAnuncios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarAnuncioPermanente(int id)
        {
            var anuncio = await _context.Anuncio.FindAsync(id);

            if (anuncio != null)
            {
                var favoritos = _context.Favorito.Where(f => f.IdAnuncio == id);
                _context.Favorito.RemoveRange(favoritos);

                var imagens = _context.Imagem.Where(i => i.IdAnuncio == id);
                _context.Imagem.RemoveRange(imagens);

                _context.Anuncio.Remove(anuncio);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(GerirAnuncios), new { filtroEstado = "Removido" });
        }

        public async Task<IActionResult> DashboardVendedores()
        {
            var pendentes = await _context.Vendedor
                .Where(v => v.IsAprovado == false)
                .ToListAsync();

            return View(pendentes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarVendedor(int id)
        {
            var vendedor = await _context.Vendedor.FindAsync(id);
            if (vendedor == null) return NotFound();

            vendedor.IsAprovado = true;
            vendedor.MotivoBloqueio = null;
            vendedor.AdminAprovadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(vendedor.UserId);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync("Vendedor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Vendedor"));
                }
                await _userManager.AddToRoleAsync(user, "Vendedor");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DashboardVendedores));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarVendedor(int id)
        {
            var vendedor = await _context.Vendedor.FindAsync(id);
            if (vendedor != null)
            {
                _context.Vendedor.Remove(vendedor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DashboardVendedores));
        }

        public IActionResult Metricas()
        {
            return View();
        }
    }
}