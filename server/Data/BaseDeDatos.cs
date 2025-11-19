using BellaVista.Models;
using Microsoft.EntityFrameworkCore;

namespace BellaVista.Data;

public class BaseDeDatos(LoginService loginService) : DbContext
{
    public DbSet<Sede> Sedes { get; set; } = default!;
    public DbSet<Evento> Eventos { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=bellavista.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sede>()
            .HasMany(x => x.Eventos)
            .WithOne(x => x.Sede!)
            .HasForeignKey(x => x.SedeId);
    }

    public void SeedData()
    {
        if (Sedes.Any())
        {
            return;
        }

        Sedes.Add(new Sede { Id = "SEDE-MAIN", Password = loginService.Hash("123456") });
        SaveChanges();
    }
}
