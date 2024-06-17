using MotoRiders.CR.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MotoRiders.CR.Models
{
    public class Cotizacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdTipoCotizacion { get; set; }

        [ForeignKey("IdTipoCotizacion")]
        public virtual TipoCotizacion TipoCotizacion { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        [Required(ErrorMessage = "El campo IdProducto es requerido.")]
        public int IdProducto { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }

        [Required(ErrorMessage = "El campo estadoCivil es requerido.")]
        public string EstadoCivil { get; set; }

        [Required(ErrorMessage = "El campo tipoVivienda es requerido.")]
        public string TipoVivienda { get; set; }

        [Required(ErrorMessage = "El campo tiempoDomicilio es requerido.")]
        public int TiempoDomicilio { get; set; }

        [Required(ErrorMessage = "El campo empresaTrabaja es requerido.")]
        public string EmpresaTrabaja { get; set; }

        [Required(ErrorMessage = "El campo tiempoEmpresa es requerido.")]
        public int TiempoEmpresa { get; set; }

        [Required(ErrorMessage = "El campo salarioMensualAproximado es requerido.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalarioMensualAproximado { get; set; }

        [Required(ErrorMessage = "El campo numeroEmpresa es requerido.")]
        public string NumeroEmpresa { get; set; }
    }
}
