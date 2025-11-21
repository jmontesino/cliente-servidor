namespace BellaVista.Models;

public class Sede
{
    public required string Id { get; set; }
    public required string Password { get; set; }
    public bool IsMain { get; set; }
    public List<Evento>? Eventos { get; set; }
}
