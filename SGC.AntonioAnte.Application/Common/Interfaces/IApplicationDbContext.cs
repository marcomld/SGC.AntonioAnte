using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Auditoria> Auditorias { get; }
        DbSet<Departamento> Departamentos { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
