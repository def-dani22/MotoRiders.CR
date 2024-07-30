using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotoRiders.CR.Models
{
    public class Contacto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Provincia { get; set; }

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; }

        [Required]
        [MaxLength(20)]
        public string Whatsapp { get; set; }

        [Required]
        [MaxLength(255)]
        public string Correo { get; set; }

        [Required]
        [MaxLength(255)]
        public string Direccion { get; set; }


        public float Latitude { get; set; }

        public float Longitude { get; set; }
    }
}
