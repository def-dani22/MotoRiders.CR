using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotoRiders.CR.Models
{
    public class PruebaManejo
    {
        public int IdTipoCotizacion { get; set; }
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

        public List<SelectListItem> TiposCotizacion { get; set; }
        public List<SelectListItem> Productos { get; set; }
    }
}
