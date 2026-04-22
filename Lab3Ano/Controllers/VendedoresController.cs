using Lab3Ano.Data;
using Lab3Ano.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Lab3Ano.Controllers
{
    [Authorize]
    public class VendedoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public VendedoresController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> TornarVendedor()
        {
            var user = await _userManager.GetUserAsync(User);

           
            var vendedorExistente = await _context.Vendedor
                                                  .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendedorExistente != null)
            {
       
                if (vendedorExistente.IsAprovado == false)
                {
                    return View("PedidoEnviado");
                }

                if (vendedorExistente.IsAprovado == true)
                {
                    return RedirectToAction("Index", "Home");
                }
            }


            var model = new Vendedor();

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber != "910000000")
            {
                model.Contactos = user.PhoneNumber;
            }


            var perfilComprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (perfilComprador != null)
            {

                if (string.IsNullOrEmpty(model.Contactos) && !string.IsNullOrEmpty(perfilComprador.Contacto))
                {
                    model.Contactos = perfilComprador.Contacto;
                }


                if (perfilComprador.Morada != "Por preencher") model.Morada = perfilComprador.Morada;
                if (perfilComprador.Localidade != "Por preencher") model.Localidade = perfilComprador.Localidade;
                if (perfilComprador.CodPostal != "0000-000") model.CodPostal = perfilComprador.CodPostal;
                model.Pais = perfilComprador.Pais;
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TornarVendedor(Vendedor vendedor)
        {
            var user = await _userManager.GetUserAsync(User);

            vendedor.Nome = user.UserName;
            vendedor.UserId = user.Id;
            vendedor.IsAprovado = false;
            vendedor.IsBloqueado = false;
            vendedor.AdminAprovadorId = null;
            vendedor.MotivoBloqueio = null;

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Nome");
            ModelState.Remove("adminAprovadorId");
            ModelState.Remove("AdminAprovador");
            ModelState.Remove("MotivoBloqueio");
            ModelState.Remove("Anuncios");

            if (ModelState.IsValid)
            {
                _context.Vendedor.Add(vendedor);

                var comprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (comprador == null)
                {
                    comprador = new Lab3Ano.Models.Entidades.Comprador
                    {
                        UserId = user.Id,
                        Nome = user.UserName.Split('@')[0], 
                        Contacto = vendedor.Contactos,      
                        Morada = vendedor.Morada,           
                        CodPostal = vendedor.CodPostal,     
                        Localidade = vendedor.Localidade,   
                        Pais = vendedor.Pais                
                    };
                    _context.Comprador.Add(comprador);
                }
                else
                {
                    comprador.Contacto = vendedor.Contactos;
                    comprador.Morada = vendedor.Morada;
                    comprador.CodPostal = vendedor.CodPostal;
                    comprador.Localidade = vendedor.Localidade;
                    comprador.Pais = vendedor.Pais;
                    _context.Update(comprador);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("PedidoEnviado");
            }

            return View(vendedor);
        }
        public IActionResult PedidoEnviado()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> GestaoVisitas()
        {
            var userId = _userManager.GetUserId(User);

            var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == userId);
            if (vendedor == null) return RedirectToAction("TornarVendedor");

            var visitas = await _context.Visita
                .Include(v => v.IdAnuncioNavigation)
                    .ThenInclude(a => a.IdCarroNavigation)
                        .ThenInclude(c => c.IdModeloNavigation) 
                .Include(v => v.IdCompradorNavigation) 
                    .ThenInclude(c => c.User) 
                .Where(v => v.IdAnuncioNavigation.IdVendedor == vendedor.Id)
                .OrderByDescending(v => v.DataHora)
                .ToListAsync();

            return View(visitas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarEstadoVisita(int idVisita, string novoEstado)
        {
            var visita = await _context.Visita.FindAsync(idVisita);
            if (visita != null)
            {
                visita.Estado = novoEstado;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(GestaoVisitas));
        }
    }
}