using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}
