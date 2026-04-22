using Lab.Models;
using Lab3Ano.Models;
using Lab3Ano.Models.Entidades;
using Lab3Ano.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;

namespace Lab3Ano.Controllers
{
    public class AnuncioController : Controller
    {
        private readonly Lab3Ano.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public AnuncioController(Lab3Ano.Data.ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        public JsonResult ObterModelosPorMarca(int idMarca)
        {
            var modelos = _context.Modelo
                .Where(m => m.IdMarca == idMarca)
                .OrderBy(m => m.Nome)
                .Select(m => new
                {
                    id = m.Id,
                    nome = m.Nome
                })
                .ToList();

            return Json(modelos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var anuncio = await _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .Include(a => a.Imagems)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdModeloNavigation)
                        .ThenInclude(m => m.IdMarcaNavigation)
                .Include(a => a.IdCarroNavigation.IdCombustivelNavigation)
                .Include(a => a.IdCarroNavigation.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anuncio == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                ViewBag.IsFavorito = await _context.Favorito
                    .AnyAsync(f => f.IdAnuncio == id && f.IdComprador == userId);

                if (anuncio.IdVendedorNavigation.UserId == userId || User.IsInRole("Admin"))
                {
                    ViewBag.IsDono = true;
                }
                else
                {
                    ViewBag.IsDono = false;
                }

                ViewBag.CurrentUserId = userId;
            }
            else
            {
                ViewBag.IsFavorito = false;
                ViewBag.IsDono = false;
            }

            ViewBag.TotalFavoritos = await _context.Favorito.CountAsync(f => f.IdAnuncio == id);
            ViewBag.CurrentUserId = _userManager.GetUserId(User);
            return View(anuncio);
        }

        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marca, "Id", "Nome");
            ViewData["IdModelo"] = new SelectList(_context.Modelo, "Id", "Nome");
            ViewData["IdCombustivel"] = new SelectList(_context.Combustivel, "Id", "Tipo");
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            try
            {
                int.TryParse(Request.Form["idMarca"], out int idMarca);
                int.TryParse(Request.Form["idModelo"], out int idModelo);
                int.TryParse(Request.Form["idCombustivel"], out int idCombustivel);
                int.TryParse(Request.Form["idCategoria"], out int idCategoria);

                int.TryParse(Request.Form["ano"], out int ano);
                int.TryParse(Request.Form["quilometragem"], out int quilometragem);

                int.TryParse(Request.Form["Potencia"], out int potencia);
                int.TryParse(Request.Form["Cilindrada"], out int cilindrada);

                string precoTexto = Request.Form["preco"].ToString().Replace(".", ",");
                decimal.TryParse(precoTexto, out decimal preco);

                string descricao = Request.Form["descricao"];
                string caixa = Request.Form["caixa"];

                if (string.IsNullOrEmpty(descricao)) return Content("ERRO: A descrição não pode estar vazia.");
                if (idMarca == 0 || idModelo == 0 || idCategoria == 0 || idCombustivel == 0) return Content("ERRO: Seleciona todos os campos obrigatórios.");
                if (string.IsNullOrEmpty(caixa)) return Content("ERRO: Seleciona a Caixa.");
                if (preco <= 0) return Content("ERRO: Preço inválido.");
                if (potencia <= 0 || cilindrada <= 0) return Content("ERRO: Potência e Cilindrada inválidas.");

                var ficheiros = Request.Form.Files;

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Content("Erro: Login necessário.");

                var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == user.Id);
                if (vendedor == null) return Content($"ERRO: Conta de Vendedor não encontrada para o User ID: {user.Id}");

                var modelo = await _context.Modelo.Include(m => m.IdMarcaNavigation).FirstOrDefaultAsync(m => m.Id == idModelo);
                string titulo = (modelo != null) ? $"{modelo.IdMarcaNavigation?.Nome} {modelo.Nome}" : "Viatura";

                var novoCarro = new Carro
                {
                    IdModelo = idModelo,
                    IdCombustivel = idCombustivel,
                    IdCategoria = idCategoria,
                    Ano = ano,
                    Preco = preco,
                    Quilometragem = quilometragem,
                    Potencia = potencia,
                    Cilindrada = cilindrada,
                    Caixa = caixa,
                    Localizacao = vendedor.Localidade ?? "PT"
                };
                _context.Carro.Add(novoCarro);
                await _context.SaveChangesAsync();

                var novoAnuncio = new Anuncio
                {
                    Titulo = titulo,
                    Descricao = descricao,
                    Estado = "Ativo",
                    DataCriacao = DateTime.Now,
                    IdVendedor = vendedor.Id,
                    IdCarro = novoCarro.Id
                };
                _context.Anuncio.Add(novoAnuncio);
                await _context.SaveChangesAsync();

                if (ficheiros.Count > 0)
                {
                    string pastaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "imagens");
                    if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

                    foreach (var foto in ficheiros)
                    {
                        if (foto.Length > 0)
                        {
                            string nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                            using (var stream = new FileStream(Path.Combine(pastaDestino, nomeFicheiro), FileMode.Create))
                            {
                                await foto.CopyToAsync(stream);
                            }
                            _context.Imagem.Add(new Imagem { IdAnuncio = novoAnuncio.Id, Url = nomeFicheiro });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                try
                {
                    if (modelo != null && modelo.IdMarcaNavigation != null)
                    {
                        var marcaId = modelo.IdMarca;
                        var nomeMarca = modelo.IdMarcaNavigation.Nome;

                        var seguidores = await _context.MarcaFavorita
                            .Where(mf => mf.MarcaId == marcaId)
                            .ToListAsync();

                        foreach (var seguidor in seguidores)
                        {
                            if (seguidor.UserId == user.Id) continue;

                            var novaNotificacao = new Notificacao
                            {
                                UserId = seguidor.UserId,
                                Texto = $"Novo {nomeMarca} disponível: {titulo}",
                                UrlDestino = $"/Anuncio/Details/{novoAnuncio.Id}",
                                Data = DateTime.Now,
                                Lida = false
                            };

                            _context.Notificacao.Add(novaNotificacao);
                        }

                        if (seguidores.Any())
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception exNotif)
                {
                    Console.WriteLine("Erro ao criar notificações: " + exNotif.Message);
                }

                TempData["Sucesso"] = "Parabéns! O teu anúncio foi publicado com sucesso.";
                return RedirectToAction("Details", new { id = novoAnuncio.Id });

            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> [CRASH] " + ex.Message);
                ModelState.AddModelError("", "Ocorreu um erro: " + ex.Message);
                ViewData["IdMarca"] = new SelectList(_context.Marca, "Id", "Nome");
                return Content($"ERRO: {ex.Message}");
            }
        }

        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var anuncio = await _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .Include(a => a.Imagems)
                .Include(a => a.IdCarroNavigation)
                    .ThenInclude(c => c.IdModeloNavigation)
                        .ThenInclude(m => m.IdMarcaNavigation)
                .Include(a => a.IdCarroNavigation.IdCategoriaNavigation)
                .Include(a => a.IdCarroNavigation.IdCombustivelNavigation)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null) return NotFound();

            bool existeVenda = await _context.Compra.AnyAsync(c => c.IdAnuncio == id);

            if (existeVenda)
            {
                TempData["Erro"] = "Bloqueado: Este carro já foi vendido e pago. Não é possível editar.";
                return Redirect("/Identity/Account/Manage");
            }

            var userId = _userManager.GetUserId(User);
            if (anuncio.IdVendedorNavigation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Content("Acesso Negado: Não podes editar anúncios de outros vendedores.");
            }

            var carro = anuncio.IdCarroNavigation;

            ViewData["IdMarca"] = new SelectList(_context.Marca, "Id", "Nome", carro.IdModeloNavigation?.IdMarca);
            var modelosDaMarca = _context.Modelo.Where(m => m.IdMarca == carro.IdModeloNavigation.IdMarca);
            ViewData["IdModelo"] = new SelectList(modelosDaMarca, "Id", "Nome", carro.IdModelo);
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "Id", "Nome", carro.IdCategoria);
            ViewData["IdCombustivel"] = new SelectList(_context.Combustivel, "Id", "Tipo", carro.IdCombustivel);

            ViewBag.MarcaSelecionada = carro.IdModeloNavigation?.IdMarca;

            return View(anuncio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<IFormFile> novasFotos)
        {
            bool existeVenda = await _context.Compra.AnyAsync(c => c.IdAnuncio == id);
            if (existeVenda)
            {
                TempData["Erro"] = "Bloqueado: Este carro já foi vendido e pago. Edição rejeitada.";
                return Redirect("/Identity/Account/Manage");
            }

            var anuncio = await _context.Anuncio
                .Include(a => a.IdCarroNavigation)
                .Include(a => a.IdVendedorNavigation)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (anuncio.IdVendedorNavigation?.UserId != userId && !User.IsInRole("Admin"))
            {
                return Content("ERRO: Não tens permissão para alterar este anúncio.");
            }

            try
            {
                string novoPrecoTexto = Request.Form["preco"].ToString().Replace(".", ",");
                if (decimal.TryParse(novoPrecoTexto, out decimal novoPreco))
                {
                    anuncio.IdCarroNavigation.Preco = novoPreco;
                }

                string descricao = Request.Form["descricao"];
                string novoEstado = Request.Form["Estado"];

                anuncio.Titulo = Request.Form["Titulo"];
                anuncio.Descricao = descricao;

                if (!string.IsNullOrEmpty(novoEstado))
                {
                    anuncio.Estado = novoEstado;
                }

                _context.Update(anuncio);
                await _context.SaveChangesAsync();

                if (novasFotos != null && novasFotos.Count > 0)
                {
                    string pastaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "imagens");
                    foreach (var foto in novasFotos)
                    {
                        if (foto.Length > 0)
                        {
                            string nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                            using (var stream = new FileStream(Path.Combine(pastaDestino, nomeFicheiro), FileMode.Create))
                            {
                                await foto.CopyToAsync(stream);
                            }
                            _context.Imagem.Add(new Imagem { IdAnuncio = anuncio.Id, Url = nomeFicheiro });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Sucesso"] = "Anúncio atualizado com sucesso!";
                return RedirectToAction("Details", new { id = anuncio.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao atualizar: " + ex.Message);
                return View(anuncio);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoverFoto(int id)
        {
            var imagem = await _context.Imagem
                .Include(i => i.IdAnuncioNavigation)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imagem == null) return Json(new { success = false, message = "Imagem não encontrada." });

            bool existeVenda = await _context.Compra.AnyAsync(c => c.IdAnuncio == imagem.IdAnuncio);
            if (existeVenda) return Json(new { success = false, message = "Carro vendido. Não pode remover fotos." });

            try
            {
                string caminhoFicheiro = Path.Combine(_webHostEnvironment.WebRootPath, "imagens", imagem.Url);
                if (System.IO.File.Exists(caminhoFicheiro))
                {
                    System.IO.File.Delete(caminhoFicheiro);
                }

                _context.Imagem.Remove(imagem);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var anuncio = await _context.Anuncio
                .Include(a => a.IdVendedorNavigation)
                .Include(a => a.Imagems)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (anuncio.IdVendedorNavigation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Content("Acesso Negado.");
            }

            try
            {
                string pastaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "imagens");
                foreach (var img in anuncio.Imagems)
                {
                    var caminho = Path.Combine(pastaDestino, img.Url);
                    if (System.IO.File.Exists(caminho)) System.IO.File.Delete(caminho);
                }
                _context.Imagem.RemoveRange(anuncio.Imagems);

                var favoritos = _context.Favorito.Where(f => f.IdAnuncio == id);
                _context.Favorito.RemoveRange(favoritos);

                var mensagens = await _context.Mensagem.Where(m => m.AnuncioId == id).ToListAsync();
                _context.Mensagem.RemoveRange(mensagens);

                var linkDestino = $"/Anuncio/Details/{id}";
                var notificacoes = _context.Notificacao.Where(n => n.UrlDestino == linkDestino);
                _context.Notificacao.RemoveRange(notificacoes);

                _context.Anuncio.Remove(anuncio);

                var carro = await _context.Carro.FindAsync(anuncio.IdCarro);
                if (carro != null) _context.Carro.Remove(carro);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Anúncio removido com sucesso!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return Content("Erro crítico ao apagar: " + ex.Message + " | " + ex.InnerException?.Message);
            }
        }
        [Authorize] 
        [HttpGet]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var anuncio = await _context.Anuncio
                .Include(a => a.IdCarroNavigation)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null) return NotFound();

            if (anuncio.Estado != "Ativo" && anuncio.Estado != "Reservado")
            {
                TempData["Erro"] = "Desculpe, este carro já foi vendido.";
                return RedirectToAction("Index", "Home");
            }

            var model = new CheckoutViewModel
            {
                IdAnuncio = anuncio.Id,
                Titulo = anuncio.Titulo,
                Preco = anuncio.IdCarroNavigation.Preco,
                MetodoPagamento = "MB Way"
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCompra(CheckoutViewModel model)
        {
            var anuncio = await _context.Anuncio.FindAsync(model.IdAnuncio);

            if (anuncio != null && (anuncio.Estado == "Ativo" || anuncio.Estado == "Reservado"))
            {

                var userIdLogin = _userManager.GetUserId(User);

                var comprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == userIdLogin);

                if (comprador == null)
                {
                    TempData["Erro"] = "Erro: Perfil de comprador não encontrado.";
                    return RedirectToAction("Index", "Home");
                }

                anuncio.Estado = "Vendido";
                _context.Update(anuncio);

                var novaCompra = new Compra
                {
                    IdAnuncio = anuncio.Id,
                    IdComprador = comprador.Id, 
                    DataCompra = DateTime.Now,
                    Valor = model.Preco
                };
                _context.Compra.Add(novaCompra);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = $"Compra efetuada com sucesso via {model.MetodoPagamento}! Parabéns.";
            }
            else
            {
                TempData["Erro"] = "Erro: O carro já não se encontra disponível.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}