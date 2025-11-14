using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace servidor;

public class Sesion(string issuer, string audience, SecurityKey securityKey)
{
    private const int saltSize = 16;
    private const int keySize = 64;
    private const int iterations = 350000;
    private readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
    
    private const char segmentDelimiter = ':';

    public String GenerateToken(string id)
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

       var Token = new JwtSecurityToken(
           issuer: issuer,
           audience: audience,
           claims: claims,
           expires: DateTime.UtcNow.AddDays(5),
           signingCredentials: signingCredentials
       );

       return new JwtSecurityTokenHandler().WriteToken(Token);
    }

    public string Hash(string input)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, hashAlgorithm, keySize);
        return string.Join(segmentDelimiter, Convert.ToHexString(hash), Convert.ToBase64String(salt));
    }
     public bool Verify(string input, string hashString )
    {
        string[] segments = hashString.Split(segmentDelimiter);
        byte[] hash = Convert.FromHexString(segments[0]);
        byte[] salt = Convert.FromHexString(segments[1]);
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, hashAlgorithm, hash.Length);
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}