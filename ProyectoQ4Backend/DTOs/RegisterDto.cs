namespace ProyectoQ4Backend.DTOs;

/**
 * DTO significa Data Transfer Object
 * Es una clase que describe SOLO los datos que el FE debe enviar
 * Id o la fecha las debe asignar - calcular el servidor
 */

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}