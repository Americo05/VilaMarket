using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Lab3Ano.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _sender;

       
        public ForgotPasswordConfirmation(UserManager<IdentityUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public void OnGet(string email)
        {
            Email = email;
        }

       
        public async Task<IActionResult> OnPostAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email },
                    protocol: Request.Scheme);

                await _sender.SendEmailAsync(
                    email,
                    "Reset Password",
                    $"Por favor recupera a tua password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");
            }

            StatusMessage = "Email reenviado com sucesso!";
            Email = email;
            return Page(); 
        }
    }
}