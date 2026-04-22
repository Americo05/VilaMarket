namespace Lab3Ano.Models
    
{
    public class UtilizadorAdminViewModel
    {
        public int Id { get; set; }             
        public string Tipo { get; set; }       
        public string Nome { get; set; }
        public string Email { get; set; }       
        public string MotivoBloqueio { get; set; }
        public bool IsBloqueado { get; set; } 


        public bool? IsAprovado { get; set; }
    }
}
