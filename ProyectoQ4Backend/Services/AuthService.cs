using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop.Infrastructure;
using ProyectoQ4Backend.DTOs;
using ProyectoQ4Backend.Models;

namespace ProyectoQ4Backend.Services;

public class AuthService
{
    private readonly FirebaseService _firebaseService;
    
    private readonly IConfiguration _configuration;

    public AuthService(FirebaseService firebaseService, IConfiguration configuration)
    {
     _firebaseService = firebaseService;
     _configuration = configuration;
    }

    public async Task<User> Register(RegisterDto dto)
    {
        var collection = _firebaseService.GetCollection("user");
        
        
        // Antes de crear, buscar el correo (que no exista)
        var existing = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (existing.Count > 0)
        {
            throw new Exception("Ya existe un usuario con ese correo");
        }

        var user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            // Hash y CreatedAt
            PasswordHash = HashPassword(dto.Password),
            CreatedAt =  DateTime.UtcNow
        };
        
        await collection.Document(user.Id).SetAsync(new Dictionary<string, object>
        {
            {"Id", user.Id},
            {"FullName", user.FullName},
            {"Email", user.Email},
            {"PasswordHash", user.PasswordHash},
            {"CreatedAt", user.CreatedAt}
        });
        return user;
    }

    public async Task<string> Login(LoginDto dto)
    {
        var collection = _firebaseService.GetCollection("user");
        
        // Antes de crear, buscar el correo (que no exista)
        var snapshot = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
        {
            throw new Exception("Credenciales invalidas");
        }

        var data = snapshot.Documents[0].ToDictionary();

        var user = new User()
        {
            Id = data["Id"].ToString()!,
            FullName = data["FullName"].ToString()!,
            Email = data["Email"].ToString()!,
            PasswordHash = data["PasswordHash"].ToString()!,
            CreatedAt = ((Google.Cloud.Firestore.Timestamp)data["CreatedAt"]).ToDateTime()
        };

        if (!VerifyPasword(dto.Password, user.PasswordHash))
            throw new Exception("Credenciales invalidas");
        
        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        // Claims - son los datos que viajan dentro del token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        // La clave secreta firma el token, si alguien lo altera, el servidor lo rechaza
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"], //quien genera el token?
            audience: _configuration["Jwt:Issuer"], //para quien es el token?
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPasword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private string HashPassword(string password)
    {
        // SHA256
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return  Convert.ToBase64String(bytes);
    }
}