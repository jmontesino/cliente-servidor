using BellaVista.Data;
using BellaVista.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using BellaVista;

const string ISSUER = "BellaVistaIssuer";
const string AUDIENCE = "BellaVistaAudience";
const string SECRET_KEY = "3aedfb879e698fb235ec1b857d5fac85ed288a6abfd9d5cbfcda80c467a6e527";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET_KEY));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BaseDeDatos>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "all", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = ISSUER,
            ValidAudience = AUDIENCE,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton(new LoginService(ISSUER, AUDIENCE, key));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var baseDeDatos = scope.ServiceProvider.GetRequiredService<BaseDeDatos>();
    baseDeDatos.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("all");

app.MapPost("/login", async ([FromServices] BaseDeDatos baseDeDatos, [FromServices] LoginService loginService, [FromBody] Sede sede) =>
{
    var hashedPassword = loginService.Hash(sede.Password);
    var sedeEncontrada = await baseDeDatos.Sedes.FirstOrDefaultAsync(x => x.Id == sede.Id);
    if (sedeEncontrada is null || !loginService.Verify(sede.Password, sedeEncontrada.Password))
    {
        return Results.Unauthorized();
    }
    var token = loginService.GenerateToken(sedeEncontrada.Id);
    return Results.Ok(new { token });
}).AllowAnonymous();

app.MapPut("/sedes", async ([FromServices] BaseDeDatos baseDeDatos, [FromServices] LoginService loginService, [FromBody] Sede sede) =>
{
    if (!await baseDeDatos.Sedes.AnyAsync(x => x.Id == sede.Id))
    {
        sede.Password = loginService.Hash(sede.Password);
        await baseDeDatos.Sedes.AddAsync(sede);
        await baseDeDatos.SaveChangesAsync();
    }

    return Results.Ok();
}).RequireAuthorization();

app.MapPut("/eventos", async ([FromServices] BaseDeDatos baseDeDatos, [FromBody] Evento evento) =>
{
    if (!await baseDeDatos.Sedes.AnyAsync(x => x.Id == evento.SedeId))
    {
        return Results.BadRequest("La sede no existe.");
    }

    if (!await baseDeDatos.Eventos.AnyAsync(x => x.Id == evento.Id))
    {
        await baseDeDatos.Eventos.AddAsync(evento);
        await baseDeDatos.SaveChangesAsync();
    }

    return Results.Ok();
}).RequireAuthorization();

app.MapGet("/eventos", async ([FromServices] BaseDeDatos baseDeDatos) =>
{
    return await baseDeDatos.Eventos.ToListAsync();
}).RequireAuthorization();

app.MapGet("/sedes/{sede}/eventos", async ([FromServices] BaseDeDatos baseDeDatos, [FromRoute] string sede) =>
{
    return await baseDeDatos.Eventos.Where(x => x.SedeId == sede).ToListAsync();
}).RequireAuthorization();


await app.RunAsync();