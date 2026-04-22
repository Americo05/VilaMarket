using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Combustivel
{
    public int Id { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual ICollection<Carro> Carros { get; set; } = new List<Carro>();
}
