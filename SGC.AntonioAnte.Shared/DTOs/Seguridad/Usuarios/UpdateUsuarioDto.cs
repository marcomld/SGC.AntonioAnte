using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios
{
    public class UpdateUsuarioDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(150)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(150)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        public Guid? DepartamentoId { get; set; }
    }
}
