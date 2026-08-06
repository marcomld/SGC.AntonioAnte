using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios
{
    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "La cédula debe contener exactamente 10 dígitos.")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo institucional es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres completos son obligatorios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos completos son obligatorios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El departamento municipal es obligatorio.")]
        public Guid? DepartamentoId { get; set; }

        [Required(ErrorMessage = "El rol base es obligatorio.")]
        public string RolAsignado { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña inicial es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}
