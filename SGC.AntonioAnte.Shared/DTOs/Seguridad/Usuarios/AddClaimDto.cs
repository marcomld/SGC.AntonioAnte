using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios
{
    public class AddClaimDto
    {
        public Guid UsuarioId { get; set; }
        public string TipoClaim { get; set; } = string.Empty;
        public string ValorClaim { get; set; } = string.Empty;
    }
}
