using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Marca
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Modelo> Modelos { get; set; } = new List<Modelo>();
}
