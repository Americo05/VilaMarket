using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3Ano.Models.Entidades
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        public int IdAnuncio { get; set; }

        public int IdComprador { get; set; }

        public DateTime DataReserva { get; set; } = DateTime.Now;
        public DateTime DataExpira { get; set; } = DateTime.Now.AddDays(3);
        public bool? IsAtiva { get; set; } = true;

        [ForeignKey("IdAnuncio")]
        [InverseProperty("Reservas")] 
        public virtual Anuncio IdAnuncioNavigation { get; set; }

        [ForeignKey("IdComprador")]
        public virtual Comprador IdCompradorNavigation { get; set; }
    }
}