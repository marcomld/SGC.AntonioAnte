using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditorias
{
    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public string IdentificacionUsuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public string? EntidadId { get; set; }
        public string DireccionIp { get; set; } = string.Empty;
        public string Navegador { get; set; } = string.Empty;
        public string DatosAdicionales { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
