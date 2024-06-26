using System;
using System.ComponentModel.DataAnnotations;

namespace MotoRiders.CR.Models
{
    public class PruebaManejo
    {
        public int IdCliente { get; set; }

        public int IdProducto { get; set; }

        public int IdContacto { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DateGreaterThanOrEqualToToday(ErrorMessage = "La fecha no puede ser anterior a hoy")]
        public DateTime Fecha { get; set; }

        [DataType(DataType.Time)]
        [Required(ErrorMessage = "La hora es obligatoria")]
        public TimeSpan Hora { get; set; }
    }

    public class DateGreaterThanOrEqualToTodayAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var fecha = (DateTime)value;
            if (fecha < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
