using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Imagem
{
    public int Id { get; set; }

    public int IdAnuncio { get; set; }

    public string Url { get; set; } = null!;

    public virtual Anuncio IdAnuncioNavigation { get; set; } = null!;
}
