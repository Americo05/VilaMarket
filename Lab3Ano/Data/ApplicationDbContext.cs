using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Lab3Ano.Models.Entidades;

namespace Lab3Ano.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Admin> Admin { get; set; }
        public virtual DbSet<Anuncio> Anuncio { get; set; }
        public virtual DbSet<Carro> Carro { get; set; }
        public virtual DbSet<Categoria> Categoria { get; set; }
        public virtual DbSet<Combustivel> Combustivel { get; set; }
        public virtual DbSet<Compra> Compra { get; set; }
        public virtual DbSet<Comprador> Comprador { get; set; }
        public virtual DbSet<Favorito> Favorito { get; set; }
        public virtual DbSet<Imagem> Imagem { get; set; }
        public virtual DbSet<Marca> Marca { get; set; }
        public virtual DbSet<Modelo> Modelo { get; set; }
        public virtual DbSet<Pesquisa> Pesquisa { get; set; }
        public virtual DbSet<Reserva> Reserva { get; set; }
        public virtual DbSet<Vendedor> Vendedor { get; set; }
        public virtual DbSet<Visita> Visita { get; set; }
        public DbSet<Mensagem> Mensagem { get; set; }
        public DbSet<Notificacao> Notificacao { get; set; }
        public DbSet<MarcaFavorita> MarcaFavorita { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<Favorito>(entity =>
            {
                entity.ToTable("Favorito");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.IdAnuncio).HasColumnName("IdAnuncio");
                entity.Property(e => e.IdComprador).HasColumnName("IdComprador");

                entity.Ignore("AnuncioId");   
                entity.Ignore("CompradorId"); 

                entity.HasOne(d => d.IdAnuncioNavigation)
                      .WithMany() 
                      .HasForeignKey(d => d.IdAnuncio)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Carro>(entity =>
            {
                entity.ToTable("Carro");

                entity.Property(e => e.Preco).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Quilometragem).HasColumnName("Quilometragem");

                entity.HasOne(d => d.IdModeloNavigation)
                      .WithMany(p => p.Carros)
                      .HasForeignKey(d => d.IdModelo)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.IdCombustivelNavigation)
                      .WithMany(p => p.Carros)
                      .HasForeignKey(d => d.IdCombustivel)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.IdCategoriaNavigation)
                      .WithMany(p => p.Carros)
                      .HasForeignKey(d => d.IdCategoria)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });


            modelBuilder.Entity<Modelo>(entity =>
            {
                entity.ToTable("Modelo");
                entity.HasOne(d => d.IdMarcaNavigation)
                      .WithMany(p => p.Modelos)
                      .HasForeignKey(d => d.IdMarca)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Anuncio>(entity =>
            {
                entity.ToTable("Anuncio");
                entity.Property(p => p.DataCriacao).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.IdCarroNavigation)
                      .WithMany(p => p.Anuncios)
                      .HasForeignKey(d => d.IdCarro);

                entity.HasOne(d => d.IdVendedorNavigation)
                      .WithMany(p => p.Anuncios)
                      .HasForeignKey(d => d.IdVendedor);
            });
            modelBuilder.Entity<Imagem>(entity =>
            {
                entity.ToTable("Imagem");

                entity.HasOne(d => d.IdAnuncioNavigation)
                      .WithMany(p => p.Imagems)
                      .HasForeignKey(d => d.IdAnuncio)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });
            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.Destinatario)
                .WithMany()
                .HasForeignKey(m => m.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Admin>(e => e.ToTable("Admin"));
            modelBuilder.Entity<Categoria>(e => e.ToTable("Categoria"));
            modelBuilder.Entity<Combustivel>(e => e.ToTable("Combustivel"));
            modelBuilder.Entity<Compra>(e => e.ToTable("Compra"));
            modelBuilder.Entity<Comprador>(e => e.ToTable("Comprador"));
            modelBuilder.Entity<Favorito>(e => e.ToTable("Favorito"));
            modelBuilder.Entity<Imagem>(e => e.ToTable("Imagem"));
            modelBuilder.Entity<Marca>(e => e.ToTable("Marca"));
            modelBuilder.Entity<Pesquisa>(e => e.ToTable("Pesquisa"));
            modelBuilder.Entity<Reserva>(e => e.ToTable("Reserva"));
            modelBuilder.Entity<Vendedor>(e => e.ToTable("Vendedor"));
            modelBuilder.Entity<Visita>(e => e.ToTable("Visita"));
        }
    }
}