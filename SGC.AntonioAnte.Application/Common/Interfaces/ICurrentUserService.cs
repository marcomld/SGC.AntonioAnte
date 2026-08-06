using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UsuarioId { get; }
        Guid? UsuarioIdGuid { get; }
        string IpAddress { get; }
        string UserAgent { get; }
    }
}
