using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditoria
{
    public class ResultadoPaginadoAuditDto
    {
        public List<AuditLogResponseDto> Items { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int RegistrosPorPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / (RegistrosPorPagina == 0 ? 1 : RegistrosPorPagina));
    }
}
