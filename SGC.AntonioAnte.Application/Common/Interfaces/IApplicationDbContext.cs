using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Auditoria> Auditorias { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
