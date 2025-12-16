using System;
using System.ComponentModel.DataAnnotations;

namespace CasoEstudio2.Models
{
    public class CasasModel
    {
        [Required(ErrorMessage = "Debe seleccionar una casa")]
        [Display(Name = "Casa")]
        public long IdCasa { get; set; }

        [Display(Name = "Descripción")]
        public string DescripcionCasa { get; set; } = string.Empty;

        [Display(Name = "Precio Mensual")]
        public decimal PrecioCasa { get; set; }

        [Required(ErrorMessage = "El nombre del usuario es requerido")]
        [StringLength(30, ErrorMessage = "El nombre no puede exceder 30 caracteres")]
        [Display(Name = "Usuario")]
        public string UsuarioAlquiler { get; set; } = string.Empty;

        [Display(Name = "Fecha de Alquiler")]
        public DateTime? FechaAlquiler { get; set; }

        // Propiedad calculada para mostrar el estado
        [Display(Name = "Estado")]
        public string Estado
        {
            get
            {
                return string.IsNullOrEmpty(UsuarioAlquiler) ? "Disponible" : "Reservada";
            }
        }

        // Propiedad para mostrar la fecha formateada
        public string FechaFormateada
        {
            get
            {
                return FechaAlquiler?.ToString("dd/MM/yyyy") ?? "N/A";
            }
        }
    }
}