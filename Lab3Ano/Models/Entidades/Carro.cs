using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab3Ano.Models.Entidades;

public partial class Carro
{
    public int Id { get; set; }

    public int IdModelo { get; set; }

    public int IdCombustivel { get; set; }

    public int IdCategoria { get; set; }
    [Range(1900, 2026, ErrorMessage = "Ano inválido")]
    public int Ano { get; set; }

    public decimal Preco { get; set; }

    public int Quilometragem { get; set; }

    public string Caixa { get; set; } = null!;

    public string Localizacao { get; set; } = null!;

    public int Cilindrada { get; set; }

    public int Potencia { get; set; }

    public virtual ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Combustivel IdCombustivelNavigation { get; set; } = null!;

    public virtual Modelo IdModeloNavigation { get; set; } = null!;
}
