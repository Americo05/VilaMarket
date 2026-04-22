using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Anuncio
{
    public int Id { get; set; }

    public int IdVendedor { get; set; }

    public int IdCarro { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descricao { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? DataCriacao { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual Carro IdCarroNavigation { get; set; } = null!;

    public virtual Vendedor IdVendedorNavigation { get; set; } = null!;

    public virtual ICollection<Imagem> Imagems { get; set; } = new List<Imagem>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Visita> Visita { get; set; } = new List<Visita>();


}
