using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using servidor;
using servidor.Data;
using servidor.Models;

const string ISSUER = "ServidorIssuer";
const string AUDIENCE = "ServidorAudience";
const string SECRET_KEY = "3aedfb879e698fb235ec1b857d5fac85ed2845645gf45645gh45545hghf546gf";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET_KEY));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BaseDeDatos>();
builder.Services.AddSingleton(new Sesion(ISSUER, AUDIENCE, key));

var app = builder.Build();

using (var scope = app.Services.CreateAsyncScope())
{
    var baseDeDatos = scope.ServiceProvider.GetRequiredService<BaseDeDatos>();
    baseDeDatos.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapPut("/sedes", async ([FromServices] BaseDeDatos baseDeDatos, [FromServices] Sesion sesion, [FromBody] Sede sede) =>
{
    if (await baseDeDatos.Sedes.AnyAsync(x => x.Id == sede.Id))
    {
        return Results.Ok();
    }

    sede.Password = sesion.Hash(sede.Password);
    await baseDeDatos.Sedes.AddAsync(sede);
    await baseDeDatos.SaveChangesAsync();

    return Results.Ok();
});

app.MapPut("/eventos", async ([FromServices] BaseDeDatos baseDeDatos, [FromBody] Evento evento) =>
{
    if (!await baseDeDatos.Sedes.AnyAsync(x => x.Id == evento.SedeId))
    {
        return Results.BadRequest("La sede no existe.");
    }

    if (await baseDeDatos.Eventos.AnyAsync(x => x.Id == evento.Id))
    {
        return Results.Ok();
    }
    
    await baseDeDatos.Eventos.AddAsync(evento);
    await baseDeDatos.SaveChangesAsync();

    return Results.Ok();
});

await app.RunAsync();
