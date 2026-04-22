using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Lab3Ano.Models.Entidades
{
    public class Mensagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Conteudo { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        public bool Lida { get; set; } = false;

        // --- RELAÇÕES (Quem envia e quem recebe) ---

        // Remetente (Quem envia)
        [Required]
        public string RemetenteId { get; set; }

        [ForeignKey("RemetenteId")]
        public IdentityUser Remetente { get; set; }

        // Destinatário (Quem recebe)
        [Required]
        public string DestinatarioId { get; set; }

        [ForeignKey("DestinatarioId")]
        public IdentityUser Destinatario { get; set; }

        // --- CONTEXTO (Sobre qual anúncio é a conversa?) ---
        public int? AnuncioId { get; set; }

        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; }
    }
}