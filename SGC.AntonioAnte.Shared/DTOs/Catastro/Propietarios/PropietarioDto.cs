using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios
{
    public class PropietarioDto
    {
        public Guid Id { get; set; }
        public TipoPropietario TipoPropietario { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? RazonSocial { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public EstadoCivil EstadoCivil { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
