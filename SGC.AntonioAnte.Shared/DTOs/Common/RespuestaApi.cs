using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Common
{
    public class RespuestaApi<T>
    {
        public T? Data { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}
