using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Domain.Avaluos.Entities;
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
        // Módulo 1: Seguridad
        DbSet<Auditoria> Auditorias { get; }
        DbSet<Departamento> Departamentos { get; }

        // Módulo 2: Catastro
        DbSet<TipoTenencia> TiposTenencia { get; }
        DbSet<TipoEstructura> TiposEstructura { get; }
        DbSet<EstadoConservacion> EstadosConservacion { get; }
        DbSet<Propietario> Propietarios { get; }
        DbSet<Predio> Predios { get; }
        DbSet<Dominio> Dominios { get; }
        DbSet<BloqueConstruccion> BloquesConstruccion { get; }

        // Módulo 4: Avalúos
        DbSet<PrecioBaseSuelo> PreciosBaseSuelo { get; }
        DbSet<CostoReposicion> CostosReposicion { get; }
        DbSet<FactorDepreciacion> FactoresDepreciacion { get; }
        DbSet<FactorAfectacion> FactoresAfectacion { get; }
        DbSet<LiquidacionPredial> LiquidacionesPrediales { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
