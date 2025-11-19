namespace BellaVista.Models;

public class Evento
{
    public required string Id { get; init; }
    public required string SedeId { get; init; }
    public Sede? Sede { get; set; }
    public required string Identificacion { get; init; }
    public required string Nombre { get; init; }
    public required TipoEvento Tipo { get; init; }
    public required DateTimeOffset Fecha { get; init; }
}
