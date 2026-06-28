using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Common.Exceptions
{
    public class LoginBloqueadoException : Exception
    {
        public int SegundosRestantes { get; }

        public LoginBloqueadoException(int segundosRestantes)
            : base("La cuenta se encuentra bloqueada temporalmente por demasiados intentos fallidos.")
        {
            SegundosRestantes = segundosRestantes;
        }
    }
}
