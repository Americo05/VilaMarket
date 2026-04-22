using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3Ano.Models.Entidades
{
    public class Visita
    {
        [Key]
        public int Id { get; set; }

        public int IdAnuncio { get; set; }
        public int IdComprador { get; set; }
        public DateTime DataHora { get; set; }
        public string? Estado { get; set; } = "Pendente";

        [ForeignKey("IdAnuncio")]
        [InverseProperty("Visita")]
        public virtual Anuncio IdAnuncioNavigation { get; set; }

        [ForeignKey("IdComprador")]
        public virtual Comprador IdCompradorNavigation { get; set; }
    }
}