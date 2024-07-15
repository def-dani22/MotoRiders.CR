using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MotoRiders.CR.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Detalles { get; set; }
        public DateTime FechaHora { get; set; }
        public string IPAddress { get; set; }
    }

}