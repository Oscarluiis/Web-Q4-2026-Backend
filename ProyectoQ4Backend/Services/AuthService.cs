using System.Security.Cryptography;
using System.Text;
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

    private string HashPassword(string password)
    {
        // SHA256
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return  Convert.ToBase64String(bytes);
    }
}