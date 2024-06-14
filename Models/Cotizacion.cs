using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MotoRiders.CR.Models
{

    public class Cotizacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdTipoCotizacion { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        [MaxLength(50)]
        public string EstadoCivil { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoVivienda { get; set; }

        [Required]
        public int TiempoDomicilio { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmpresaTrabaja { get; set; }

        [Required]
        public int TiempoEmpresa { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalarioMensualAproximado { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroEmpresa { get; set; }

        // Navigation properties for foreign keys
        [ForeignKey("IdTipoCotizacion")]
        public virtual TipoCotizacion TipoCotizacion { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Productos { get; set; }
    }
}