using Lab3Ano.Models.Entidades;

namespace Lab3Ano.Models.ViewModels
{
    public class PerfilPublicoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; } 
        public string TipoUtilizador { get; set; } 
        public string Localidade { get; set; }
        public string FotoPerfilUrl { get; set; }
        public DateTime DataRegisto { get; set; }
        public bool IsAprovado { get; set; }
        public List<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
        public int TotalAnuncios { get; set; }
    }
}