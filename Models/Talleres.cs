using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MotoRiders.CR.Models
{
    public class Talleres
    {
        [Key] // Indica que la propiedad Id es la clave primaria
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indica que la base de datos generará automáticamente el valor para esta propiedad
        public int Id { get; set; }

        [Required] // Indica que el campo Nombre es obligatorio
        [MaxLength(100)] // Define la longitud máxima para el campo Nombre en la base de datos
        public string Nombre { get; set; }

        [MaxLength(255)] // Define la longitud máxima para el campo Ubicacion en la base de datos
        public string Ubicacion { get; set; }

        [MaxLength(255)] // Define la longitud máxima para el campo Horarios en la base de datos
        public string Horarios { get; set; }

        [MaxLength(20)] // Define la longitud máxima para el campo Telefono en la base de datos
        public string Telefono { get; set; }
    }
}
