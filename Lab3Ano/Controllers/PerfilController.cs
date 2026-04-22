using Lab3Ano.Data;
using Lab3Ano.Models.Entidades;
using Lab3Ano.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab3Ano.Controllers
{
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PerfilController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Ver(int id, string tipo)
        {
            string userIdReal = "";

            // =============================================================
            // 1. DESCOBRIR QUEM É O UTILIZADOR (UserId)
            // =============================================================

            // Tenta encontrar o ID na tabela que o link diz
            if (tipo == "Vendedor")
            {
                var v = await _context.Vendedor.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (v != null) userIdReal = v.UserId;
            }
            else
            {
                // Se for Comprador ou Admin, procura primeiro na tabela Comprador
                var c = await _context.Comprador.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (c != null) userIdReal = c.UserId;

                // Fallback: Se o link diz Admin mas não achou em Comprador, tenta em Vendedor
                if (string.IsNullOrEmpty(userIdReal) && tipo == "Admin")
                {
                    var vFallback = await _context.Vendedor.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (vFallback != null) userIdReal = vFallback.UserId;
                }
            }

            if (string.IsNullOrEmpty(userIdReal)) return NotFound("Utilizador não encontrado.");

            // =============================================================
            // 2. CALCULAR O "CARGO REAL" (HIERARQUIA RÍGIDA)
            // =============================================================

            var userLogin = await _userManager.FindByIdAsync(userIdReal);
            if (userLogin == null) return NotFound();

            // Verifica se existe ficha de vendedor (INDEPENDENTEMENTE DE ESTAR APROVADO OU NÃO)
            var fichaVendedor = await _context.Vendedor
                .Include(v => v.Anuncios).ThenInclude(a => a.Imagems)
                .FirstOrDefaultAsync(v => v.UserId == userIdReal);

            // MUDANÇA AQUI: Removemos o "&& fichaVendedor.IsAprovado == true"
            // Agora basta existir a ficha para ser considerado Vendedor visualmente
            bool existeFichaVendedor = (fichaVendedor != null);
            bool isAdmin = await _userManager.IsInRoleAsync(userLogin, "Admin");

            string cargoReal = "Comprador";

            if (isAdmin)
            {
                cargoReal = "Admin";
            }
            else if (existeFichaVendedor) 
            {
                cargoReal = "Vendedor";
            }
            else
            {
                cargoReal = "Comprador";
            }

            // =============================================================
            // 3. O "SEGURANÇA DA PORTA" (BLOQUEIO)
            // =============================================================

            // Se o tipo pedido no link for diferente do cargo real -> BLOQUEIA
            // Exemplo: Se és Vendedor e tentas entrar como Comprador -> 404
            if (tipo != cargoReal)
            {
                return NotFound();
            }

            // =============================================================
            // 4. PREENCHER O MODELO (Agora sabemos que o tipo está certo)
            // =============================================================

            var model = new PerfilPublicoViewModel();
            model.Id = id;
            model.Email = userLogin.Email;
            model.TipoUtilizador = cargoReal;

            if (cargoReal == "Admin")
            {
                model.Nome = "Administrador " + userLogin.UserName;
                model.Localidade = "Staff AutoMarket";
                model.Anuncios = new List<Anuncio>();
            }
            else if (cargoReal == "Vendedor")
            {
                model.Nome = fichaVendedor.Nome;
                model.Localidade = fichaVendedor.Localidade ?? "Portugal";
                // Os dois pontos de interrogação significam: "Se for nulo, usa false"
                model.IsAprovado = fichaVendedor.IsAprovado ?? false;

                model.Anuncios = await _context.Anuncio
                    .Include(a => a.Imagems)
                    .Include(a => a.IdCarroNavigation).ThenInclude(m => m.IdModeloNavigation).ThenInclude(m => m.IdMarcaNavigation)
                    .Where(a => a.IdVendedor == fichaVendedor.Id && (a.Estado == "Ativo" || a.Estado == null))
                    .OrderByDescending(a => a.DataCriacao)
                    .ToListAsync();
                model.TotalAnuncios = model.Anuncios.Count;
            }
            else // Comprador
            {
                var fichaComprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == userIdReal);
                model.Nome = fichaComprador?.Nome ?? userLogin.UserName;
                model.Localidade = "Portugal";
                model.Anuncios = new List<Anuncio>();
            }

            return View(model);
        }
    }
}