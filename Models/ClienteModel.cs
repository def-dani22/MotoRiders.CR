using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MotoRiders.CR.Models
{
    public class ClienteModel
    {
        [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int idCliente { get; set; }

    [Required]
    [MaxLength(20)]
    public string cedula { get; set; }

    [Required]
    [MaxLength(100)]
    public string nombre { get; set; }

    [Required]
    [MaxLength(255)]
    public string direccion { get; set; }

    [Required]
    [MaxLength(20)]
    public string telefono { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{14,20}$",
        ErrorMessage = "La contraseña debe tener entre 14 y 20 caracteres, incluir al menos una letra minúscula, una letra mayúscula, un número y un carácter especial.")]
    public string contraseña { get; set; }

    [DataType(DataType.Password)]
    [Compare("contraseña", ErrorMessage = "Las contraseñas no coinciden.")]
    public string confirmarContraseña { get; set; }

    [Required]
    [MaxLength(255)]
    public string preguntaSeguridad1 { get; set; }

    [Required]
    [MaxLength(255)]
    public string respuestaSeguridad1 { get; set; }

    [Required]
    [MaxLength(255)]
    public string preguntaSeguridad2 { get; set; }

    [Required]
    [MaxLength(255)]
    public string respuestaSeguridad2 { get; set; }

    public int intentosFallidos { get; set; }

    public DateTime? tiempoBloqueo { get; set; }

    // Datos de ubicación
    public int IdPais { get; set; }

    public int idProvincia { get; set; }

    public int idCanton { get; set; }

    public int idDistrito { get; set; }
}
}