using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Lab3Ano.Models.Entidades
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        public string Texto { get; set; } // "Nova mensagem" ou "Novo BMW disponível"
        public string? UrlDestino { get; set; } // O link para onde vai
        public DateTime Data { get; set; } = DateTime.Now;
        public bool Lida { get; set; } = false;

        // Ligação ao Utilizador (AspNetUsers) que já tens
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}