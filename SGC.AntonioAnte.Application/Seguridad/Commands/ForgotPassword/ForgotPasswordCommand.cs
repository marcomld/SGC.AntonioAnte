using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<String>
    {
        public string Email { get; set; } = string.Empty;
    }
}
