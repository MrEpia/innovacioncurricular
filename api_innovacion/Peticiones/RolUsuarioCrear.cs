using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>POST /api/rol_usuario: asignar un rol a un usuario.</summary>
public class RolUsuarioCrear
{
    [Required(ErrorMessage = "El campo usuarioId es obligatorio.")]
    public int? UsuarioId { get; set; }

    [Required(ErrorMessage = "El campo rolId es obligatorio.")]
    public int? RolId { get; set; }
}
