using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios
{
    public class UserPermissionDto
    {
        public string TipoClaim { get; set; } = string.Empty;
        public string ValorClaim { get; set; } = string.Empty;
    }
}