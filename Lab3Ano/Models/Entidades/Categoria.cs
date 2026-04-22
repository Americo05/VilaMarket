using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Carro> Carros { get; set; } = new List<Carro>();
}
