using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class DominioDto
    {
        public Guid Id { get; set; }
        public Guid PredioId { get; set; }
        public Guid PropietarioId { get; set; }
        public string IdentificacionPropietario { get; set; } = string.Empty;
        public string NombrePropietario { get; set; } = string.Empty;
        public Guid TipoTenenciaId { get; set; }
        public string NombreTipoTenencia { get; set; } = string.Empty;
        public decimal PorcentajePropiedad { get; set; }
        public DateTime? FechaInscripcion { get; set; }
        public string? Notaria { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
