using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace Lab3Ano.Models.Entidades;

public partial class Comprador
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public string? MotivoBloqueio { get; set; }

    public bool IsBloqueado { get; set; }

    public string? Morada { get; set; }

    public string? Localidade { get; set; }

    public string? CodPostal { get; set; }

    public string? Pais { get; set; }

    public string? Contacto { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    

    public virtual ICollection<Pesquisa> Pesquisas { get; set; } = new List<Pesquisa>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; }

    public virtual ICollection<Visita> Visita { get; set; } = new List<Visita>();
}
