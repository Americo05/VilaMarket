using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Modelo
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public int IdMarca { get; set; }

    public virtual ICollection<Carro> Carros { get; set; } = new List<Carro>();

    public virtual Marca IdMarcaNavigation { get; set; } = null!;
}
