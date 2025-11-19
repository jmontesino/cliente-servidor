using System.Text.Json.Serialization;

namespace BellaVista.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoEvento
{
    Entrada,
    Salida
}