namespace Lab3Ano.Models
{
    public class AnuncioAdminViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string NomeVendedor { get; set; }
        public int IdVendedor { get; set; }
        public decimal Preco { get; set; }
        public string Estado { get; set; }
        public DateTime? DataCriacao { get; set; }
        public string ImagemCapaUrl { get; set; }
    }
}