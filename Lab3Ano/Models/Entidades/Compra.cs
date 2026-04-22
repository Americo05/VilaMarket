using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3Ano.Models.Entidades
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        public int IdAnuncio { get; set; }
        public int IdComprador { get; set; }
        public DateTime DataCompra { get; set; } = DateTime.Now;
        public decimal Valor { get; set; }
        public string? EstadoPagamento { get; set; }

        [ForeignKey("IdAnuncio")]
        [InverseProperty("Compras")] 
        public virtual Anuncio IdAnuncioNavigation { get; set; }

        [ForeignKey("IdComprador")]
        public virtual Comprador IdCompradorNavigation { get; set; }
    }
}