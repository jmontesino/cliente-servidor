using Microsoft.EntityFrameworkCore;
using servidor.Models;

namespace servidor.Data;

public class BaseDeDatos : DbContext
{
    public DbSet<Sede> Sedes { get; set; } = default!;
    public DbSet<Evento> Eventos { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=servidor.db");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sede>()
            .HasMany(X => X.Eventos)
            .WithOne(X => X.Sede!)
            .HasForeignKey(X => X.SedeId);
    }
}