using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios
{
    public class CreatePropietarioDto
    {
        [Required(ErrorMessage = "El tipo de propietario es obligatorio (1 = Natural, 2 = Juridico).")]
        public TipoPropietario TipoPropietario { get; set; }

        [Required(ErrorMessage = "La identificación (Cédula o RUC) es totalmente obligatoria.")]
        [StringLength(20, ErrorMessage = "La identificación no puede exceder los 20 caracteres.")]
        public string Identificacion { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Los nombres no pueden exceder los 100 caracteres.")]
        public string? Nombres { get; set; }

        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
        public string? Apellidos { get; set; }

        [StringLength(200, ErrorMessage = "La razón social no puede exceder los 200 caracteres.")]
        public string? RazonSocial { get; set; }

        public EstadoCivil EstadoCivil { get; set; } = EstadoCivil.Soltero;

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres.")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        public string? Telefono { get; set; }
    }
}
