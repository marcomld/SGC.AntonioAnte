using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles
{
    public class AssignRoleDto
    {
        public Guid UsuarioId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}
