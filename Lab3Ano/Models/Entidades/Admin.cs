using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Lab3Ano.Models.Entidades;

public partial class Admin
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? Nome { get; set; }

    public virtual IdentityUser User { get; set; } = null!;
}
