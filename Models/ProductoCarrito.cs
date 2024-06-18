using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotoRiders.CR.Models
{
    public class ProductoCarrito
    {
        [Key]
        public int Id { get; set; }

        public int CarritoId { get; set; }

        [ForeignKey("CarritoId")]
        public virtual CarritoCompra Carrito { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        public int Cantidad { get; set; }
    }
}
