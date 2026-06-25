using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.CreateUsuario
{
    public class CreateUsuarioCommand : IRequest<Guid>
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;

        // La contraseña inicial que se le asignará al funcionario
        public string Password { get; set; } = string.Empty;

        // El nombre del rol base que queremos asignarle (ej. "TecnicoCatastral")
        public string RolAsignado { get; set; } = string.Empty;
    }
}
