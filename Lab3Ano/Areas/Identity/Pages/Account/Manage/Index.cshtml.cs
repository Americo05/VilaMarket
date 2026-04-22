using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab3Ano.Models.Entidades;
using Lab3Ano.Data;

namespace Lab3Ano.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string DisplayName { get; set; }
        public bool IsTwoFactorEnabled { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public IList<Anuncio> MeusAnuncios { get; set; }
        public IList<Reserva> MinhasReservas { get; set; }  
        public IList<Compra> FaturasHistorico { get; set; } 
        public IList<Compra> MinhasVendas { get; set; }     
        public IList<Reserva> PedidosRecebidos { get; set; }

        public class InputModel
        {
            [Display(Name = "Nome de Utilizador")]
            [Required(ErrorMessage = "O nome de utilizador é obrigatório.")]
            public string Username { get; set; }

            [Display(Name = "Telemóvel")]
            [Phone]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Email")]
            [EmailAddress]
            public string Email { get; set; }

            [Display(Name = "Morada")]
            public string? Morada { get; set; }

            [Display(Name = "Código Postal")]
            public string? CodPostal { get; set; }

            [Display(Name = "Localidade")]
            public string? Localidade { get; set; }

            [Display(Name = "País")]
            public string? Pais { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Password Atual")]
            public string OldPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Nova Password")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Nova Password")]
            [Compare("NewPassword", ErrorMessage = "As passwords não coincidem.")]
            public string ConfirmPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Password para confirmar eliminação")]
            public string DeletePassword { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            DisplayName = userName;
            IsTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            if (Input == null)
            {
                Input = new InputModel
                {
                    Username = userName,
                    PhoneNumber = phoneNumber,
                    Email = email
                };
            }


            var comprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (comprador != null)
            {
                Input.Morada = comprador.Morada;
                Input.CodPostal = comprador.CodPostal;
                Input.Localidade = comprador.Localidade;
                Input.Pais = comprador.Pais;

                MinhasReservas = await _context.Reserva
                    .Include(r => r.IdAnuncioNavigation)
                        .ThenInclude(a => a.IdCarroNavigation)
                            .ThenInclude(car => car.IdModeloNavigation)
                                .ThenInclude(m => m.IdMarcaNavigation)
                    .Where(r => r.IdComprador == comprador.Id
                             && r.IdAnuncioNavigation.Estado != "Vendido")
                    .OrderByDescending(r => r.DataReserva)
                    .ToListAsync();

                FaturasHistorico = await _context.Compra
                    .Include(c => c.IdAnuncioNavigation)
                        .ThenInclude(a => a.IdCarroNavigation)
                            .ThenInclude(car => car.IdModeloNavigation)
                                .ThenInclude(m => m.IdMarcaNavigation)
                    .Where(c => c.IdComprador == comprador.Id)
                    .OrderByDescending(c => c.DataCompra)
                    .ToListAsync();
            }
            else
            {
                MinhasReservas = new List<Reserva>();
                FaturasHistorico = new List<Compra>();
            }

            var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendedor != null)
            {
                MeusAnuncios = await _context.Anuncio
                    .Include(a => a.IdCarroNavigation)
                        .ThenInclude(c => c.IdModeloNavigation)
                            .ThenInclude(m => m.IdMarcaNavigation)
                    .Include(a => a.Visita)
                    .Include(a => a.Imagems)
                    .Include(a => a.Compras)
                    .Where(a => a.IdVendedor == vendedor.Id)
                    .OrderByDescending(a => a.DataCriacao)
                    .ToListAsync();

                MinhasVendas = await _context.Compra
                    .Include(c => c.IdAnuncioNavigation)
                        .ThenInclude(a => a.IdCarroNavigation)
                            .ThenInclude(car => car.IdModeloNavigation)
                    .Include(c => c.IdCompradorNavigation)
                        .ThenInclude(comp => comp.User)
                    .Where(c => c.IdAnuncioNavigation.IdVendedor == vendedor.Id)
                    .OrderByDescending(c => c.DataCompra)
                    .ToListAsync();

                PedidosRecebidos = await _context.Reserva
                 .Include(r => r.IdCompradorNavigation).ThenInclude(c => c.User)
                 .Include(r => r.IdAnuncioNavigation) 
                     .ThenInclude(a => a.IdCarroNavigation)
                         .ThenInclude(m => m.IdModeloNavigation)
                 .Where(r => r.IdAnuncioNavigation.IdVendedor == vendedor.Id
                          && r.IsAtiva == false
                          && r.IdAnuncioNavigation.Estado != "Vendido") 
                 .OrderBy(r => r.DataReserva)
                 .ToListAsync();
            }
            else
            {
                MeusAnuncios = new List<Anuncio>();
                MinhasVendas = new List<Compra>();
                PedidosRecebidos = new List<Reserva>();
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Erro ao carregar utilizador.");
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Erro ao carregar utilizador.");

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

            var currentUsername = await _userManager.GetUserNameAsync(user);
            if (Input.Username != currentUsername)
                await _userManager.SetUserNameAsync(user, Input.Username);

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
                await _userManager.SetEmailAsync(user, Input.Email);

            string nomeProvisorio = Input.Username ?? user.UserName.Split('@')[0];

            var comprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (comprador != null)
            {
                comprador.Nome = nomeProvisorio;
                comprador.Contacto = Input.PhoneNumber;
                comprador.Morada = Input.Morada;
                comprador.CodPostal = Input.CodPostal;
                comprador.Localidade = Input.Localidade;
                comprador.Pais = Input.Pais;
                _context.Update(comprador);
            }
            else
            {
                var novoComprador = new Comprador
                {
                    UserId = user.Id,
                    Nome = nomeProvisorio,
                    Contacto = Input.PhoneNumber,
                    Morada = Input.Morada,
                    Localidade = Input.Localidade,
                    CodPostal = Input.CodPostal,
                    Pais = Input.Pais
                };
                _context.Comprador.Add(novoComprador);
            }

            var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendedor != null)
            {
                vendedor.Nome = nomeProvisorio;
                vendedor.Contactos = Input.PhoneNumber;
                vendedor.Morada = Input.Morada;
                vendedor.CodPostal = Input.CodPostal;
                vendedor.Localidade = Input.Localidade;
                vendedor.Pais = Input.Pais;
                _context.Update(vendedor);
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Perfil atualizado com sucesso!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Erro.");
            if (string.IsNullOrEmpty(Input.OldPassword) || string.IsNullOrEmpty(Input.NewPassword)) return RedirectToPage();
            var result = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded) { foreach (var error in result.Errors) StatusMessage += error.Description; return RedirectToPage(); }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Password alterada.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisable2FAAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Erro.");
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Erro.");
            if (!await _userManager.CheckPasswordAsync(user, Input.DeletePassword)) return RedirectToPage();

            var vendedor = await _context.Vendedor.FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendedor != null) _context.Vendedor.Remove(vendedor);

            var comprador = await _context.Comprador.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (comprador != null) _context.Comprador.Remove(comprador);

            await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }
    }
}