using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles
{
    public class PermissionDto
    {
        public string Modulo { get; set; } = string.Empty;
        public string TipoClaim { get; set; } = string.Empty;
        public string ValorClaim { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Estado en tiempo de ejecución para las Modales de Blazor
        public bool EstaActivo { get; set; }
        public bool EsHeredadoDeRol { get; set; }
        public string? NombreRolOrigen { get; set; }
    }
}
