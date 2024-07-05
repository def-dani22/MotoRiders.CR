using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace MotoRiders.CR.Models
{
    public class Token
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public string TokenValue { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

}