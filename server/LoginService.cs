using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BellaVista;

public class LoginService(string issuer, string audience, SecurityKey securityKey)
{
    public string GenerateToken(string id)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(5),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // --- Método para Hashing (Registro/Creación de Usuario) ---
    /// <summary>
    /// Hashea la contraseña para su almacenamiento seguro en la base de datos.
    /// </summary>
    /// <param name="password">La contraseña en texto plano.</param>
    /// <returns>La contraseña hasheada (que ya incluye el salt).</returns>
    public string Hash(string password)
    {
        // BCrypt.HashPassword genera automáticamente un salt único 
        // y lo incluye en el resultado hasheado.
        try
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        catch (Exception ex)
        {
            // Manejo de errores
            Console.WriteLine($"Error al hashear la contraseña: {ex.Message}");
            throw;
        }
    }

    // --- Método para Verificación (Inicio de Sesión) ---
    /// <summary>
    /// Verifica si la contraseña de texto plano coincide con el hash almacenado.
    /// </summary>
    /// <param name="providedPassword">La contraseña ingresada por el usuario.</param>
    /// <param name="hashedPassword">El hash recuperado de la base de datos.</param>
    /// <returns>True si las contraseñas coinciden, de lo contrario, False.</returns>
    public bool Verify(string providedPassword, string hashedPassword)
    {
        if (string.IsNullOrEmpty(providedPassword) || string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }

        try
        {
            // BCrypt.Verify toma la contraseña de texto plano y el hash, 
            // extrae el salt del hash, y luego hashea la contraseña de nuevo 
            // con ese salt para compararlos de forma segura.
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Esto puede ocurrir si el hash almacenado es inválido o no tiene el formato correcto.
            Console.WriteLine("Error: El hash almacenado no es válido.");
            return false;
        }
        catch (Exception ex)
        {
            // Manejo general de errores
            Console.WriteLine($"Error durante la verificación: {ex.Message}");
            return false;
        }
    }
}
