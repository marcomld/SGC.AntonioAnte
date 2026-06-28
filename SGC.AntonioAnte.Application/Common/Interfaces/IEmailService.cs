using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string paraEmail, string asunto, string cuerpoHtml);
    }
}
