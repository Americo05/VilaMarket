using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3Ano.Models.Entidades
{
    [Table("Favorito")]
    public partial class Favorito
    {
        [Key]
        public int Id { get; set; }

        public string IdComprador { get; set; }

        public int IdAnuncio { get; set; }

        [ForeignKey("IdAnuncio")]
        public virtual Anuncio IdAnuncioNavigation { get; set; } = null!;
    }
}