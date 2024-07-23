using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotoRiders.CR.Models
{
    public class CarritoCompra
    {
        [Key]
        public int Id { get; set; }

        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual ClienteModel Cliente { get; set; }

        public virtual ICollection<ProductoCarrito> ProductosCarrito { get; set; }

        [Required(ErrorMessage = "El nombre de la tarjeta es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la tarjeta no puede tener más de 100 caracteres.")]
        public string NombreTarjeta { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
        [StringLength(20, ErrorMessage = "El número de tarjeta no puede tener más de 20 caracteres.")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "La fecha de expiración es obligatoria.")]
        [StringLength(7, ErrorMessage = "La fecha de expiración debe tener el formato MM/AAAA.")]
        public string FechaExpiracion { get; set; }

        [Required(ErrorMessage = "El CVV es obligatorio.")]
        [StringLength(4, ErrorMessage = "El CVV debe tener exactamente 4 caracteres.")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio.")]
        [StringLength(50, ErrorMessage = "El número de cuenta no puede tener más de 50 caracteres.")]
        public string NumeroCuenta { get; set; }
    }
}
