 using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Seguimientos
{
     
    public class FechaMayorQueActualAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
             
            if (value == null)
                return ValidationResult.Success;

            
            if (value is DateTime fecha)
            {
                if (fecha <= DateTime.Now)
                {
                    return new ValidationResult(
                        ErrorMessage ?? "La fecha debe ser posterior a la fecha actual.");
                }
                return ValidationResult.Success;
            }
 
            return ValidationResult.Success;
        }
    }
}