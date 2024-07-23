using System;

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