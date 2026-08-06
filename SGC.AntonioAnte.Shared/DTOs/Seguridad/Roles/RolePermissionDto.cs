using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles
{
    public class RolePermissionDto
    {
        public Guid RolId { get; set; }
        public List<PermissionDto> Permisos { get; set; } = new();
    }
}
