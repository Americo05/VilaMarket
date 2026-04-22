using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
namespace Lab3Ano.Models.Entidades;

public partial class Vendedor
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public string? MotivoBloqueio { get; set; }

    public bool IsBloqueado { get; set; }

    public string Nif { get; set; } = null!;

    public string? TipoEmp { get; set; }

    public string? Contactos { get; set; }

    public string? Morada { get; set; }

    public string? Localidade { get; set; }

    public string? CodPostal { get; set; }

    public string Pais { get; set; } = null!;

    public bool? IsAprovado { get; set; }

    public string? AdminAprovadorId { get; set; }

    public virtual IdentityUser AdminAprovador { get; set; }

    public virtual ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();

    public virtual IdentityUser User { get; set; } = null!;
}
