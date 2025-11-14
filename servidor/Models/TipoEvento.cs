using System.Text.Json.Serialization;

namespace servidor.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoEvento
{
    Entrada,
    Salida
}
