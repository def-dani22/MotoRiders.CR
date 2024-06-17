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
        [MaxLength(255)]
        public string contraseña { get; set; }
    }
}
