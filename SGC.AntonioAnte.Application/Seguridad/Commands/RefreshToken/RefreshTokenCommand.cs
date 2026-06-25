using MediatR;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<TokenResponseDto>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
