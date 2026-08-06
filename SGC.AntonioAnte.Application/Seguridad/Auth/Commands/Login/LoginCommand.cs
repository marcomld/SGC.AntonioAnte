using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands.Login
{
    // Ahora retorna TokenResponseDto en lugar de string
    public class LoginCommand : IRequest<TokenResponseDto>
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
