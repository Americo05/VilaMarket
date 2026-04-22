using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Pesquisa
{
    public int Id { get; set; }

    public int IdComprador { get; set; }

    public string? FiltrosGuardados { get; set; }

    public virtual Comprador IdCompradorNavigation { get; set; } = null!;
}
