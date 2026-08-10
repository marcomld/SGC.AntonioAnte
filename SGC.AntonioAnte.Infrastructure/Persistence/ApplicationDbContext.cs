using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, Guid>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        // =========================================================================
        // INTERCEPTOR DE AUDITORÍA AUTOMÁTICA (DELTA AUDIT)
        // =========================================================================
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entradasRastreadas = ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                            && e.Entity is not Auditoria
                            && !EsTablaInternaIdentity(e.Entity))
                .ToList();

            var nuevasAuditorias = new List<Auditoria>();

            var propiedadesOmitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ConcurrencyStamp",
                "SecurityStamp",
                "NormalizedEmail",
                "NormalizedUserName",
                "PasswordHash",
                "AccessFailedCount",
                "LockoutEnd",
                "LockoutEnabled",
                "TwoFactorEnabled",
                "PhoneNumberConfirmed",
                "EmailConfirmed"
            };

            foreach (var entry in entradasRastreadas)
            {
                string nombreEntidad = entry.Entity.GetType().Name;

                if (nombreEntidad.Contains("_"))
                    nombreEntidad = nombreEntidad.Split('_')[0];

                var idPropiedad = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue?.ToString() ?? "N/A";

                string accion = string.Empty;
                string datosAdicionales = string.Empty;

                switch (entry.State)
                {
                    // 🟢 1. CREADO / INSERT
                    case EntityState.Added:
                        accion = $"CREAR_{nombreEntidad.ToUpper()}";
                        var camposCreados = entry.Properties
                            .Where(p => !propiedadesOmitidas.Contains(p.Metadata.Name) && p.CurrentValue != null)
                            .Select(p => FormatearCampoCreado(p, entry.Entity));

                        if (!camposCreados.Any()) continue;

                        datosAdicionales = $"Registro creado: {string.Join(" | ", camposCreados)}";
                        break;

                    // 🔵 2. ACTUALIZADO / UPDATE / CAMBIO DE ESTADO
                    case EntityState.Modified:
                        accion = $"ACTUALIZAR_{nombreEntidad.ToUpper()}";
                        var cambios = new List<string>();
                        bool esCambioEstadoExclusivo = false;
                        bool nuevoEstadoActivo = false;

                        foreach (var propiedad in entry.Properties)
                        {
                            if (propiedad.IsModified && !propiedadesOmitidas.Contains(propiedad.Metadata.Name))
                            {
                                var valorAnterior = propiedad.OriginalValue ?? "null";
                                var valorNuevo = propiedad.CurrentValue ?? "null";

                                if (valorAnterior.ToString() != valorNuevo.ToString())
                                {
                                    cambios.Add(FormatearCampoModificado(propiedad, entry.Entity));

                                    // Detectamos si cambió 'EstadoActivo'
                                    if (propiedad.Metadata.Name.Equals("EstadoActivo", StringComparison.OrdinalIgnoreCase))
                                    {
                                        esCambioEstadoExclusivo = true;
                                        nuevoEstadoActivo = Convert.ToBoolean(propiedad.CurrentValue);
                                    }
                                }
                            }
                        }

                        if (!cambios.Any()) continue;

                        // 🎯 Si solo cambió el EstadoActivo, etiquetamos la acción como ACTIVAR o DESACTIVAR
                        if (cambios.Count == 1 && esCambioEstadoExclusivo)
                        {
                            accion = nuevoEstadoActivo
                                ? $"ACTIVAR_{nombreEntidad.ToUpper()}"
                                : $"DESACTIVAR_{nombreEntidad.ToUpper()}";
                        }

                        datosAdicionales = $"Cambios aplicados: {string.Join(" | ", cambios)}";
                        break;

                    // 🔴 3. ELIMINADO / DELETE
                    case EntityState.Deleted:
                        accion = $"ELIMINAR_{nombreEntidad.ToUpper()}";
                        var valoresEliminados = entry.Properties
                            .Where(p => !propiedadesOmitidas.Contains(p.Metadata.Name) && p.OriginalValue != null)
                            .Select(p => FormatearCampoEliminado(p, entry.Entity));

                        datosAdicionales = $"Registro eliminado: {string.Join(" | ", valoresEliminados)}";
                        break;
                }

                nuevasAuditorias.Add(new Auditoria
                {
                    UsuarioId = _currentUserService.UsuarioIdGuid,
                    Accion = accion,
                    Entidad = nombreEntidad,
                    EntidadId = idPropiedad,
                    DatosAdicionales = datosAdicionales,
                    DireccionIp = _currentUserService.IpAddress,
                    Navegador = _currentUserService.UserAgent,
                    FechaCreacion = DateTime.UtcNow
                });
            }

            if (nuevasAuditorias.Any())
            {
                Auditorias.AddRange(nuevasAuditorias);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // =========================================================================
        // MÉTODOS DE FORMATEO Y TRADUCCIÓN DE CLAVES FORÁNEAS A TEXTO COMPRENSIBLE
        // =========================================================================
        private static string FormatearCampoCreado(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry propiedad, object entidad)
        {
            if (propiedad.Metadata.Name.Equals("DepartamentoId", StringComparison.OrdinalIgnoreCase) && entidad is Usuario usuario)
            {
                string nombreDepartamento = usuario.Departamento?.Nombre ?? "Sin Departamento Asignado";
                return $"Departamento: '{nombreDepartamento}'";
            }

            return $"{propiedad.Metadata.Name}: '{propiedad.CurrentValue}'";
        }

        private static string FormatearCampoModificado(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry propiedad, object entidad)
        {
            var valorAnterior = propiedad.OriginalValue ?? "null";
            var valorNuevo = propiedad.CurrentValue ?? "null";

            if (propiedad.Metadata.Name.Equals("DepartamentoId", StringComparison.OrdinalIgnoreCase) && entidad is Usuario usuario)
            {
                string nombreDepartamentoNuevo = usuario.Departamento?.Nombre ?? (valorNuevo.ToString() == "null" ? "Sin Departamento" : valorNuevo.ToString()!);
                return $"Departamento: cambiado a '{nombreDepartamentoNuevo}'";
            }

            return $"{propiedad.Metadata.Name}: '{valorAnterior}' -> '{valorNuevo}'";
        }

        private static string FormatearCampoEliminado(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry propiedad, object entidad)
        {
            if (propiedad.Metadata.Name.Equals("DepartamentoId", StringComparison.OrdinalIgnoreCase) && entidad is Usuario usuario)
            {
                string nombreDepartamento = usuario.Departamento?.Nombre ?? "Sin Departamento";
                return $"Departamento: '{nombreDepartamento}'";
            }

            return $"{propiedad.Metadata.Name}: '{propiedad.OriginalValue}'";
        }

        private static bool EsTablaInternaIdentity(object entity)
        {
            var tipo = entity.GetType();
            return tipo.Name.StartsWith("IdentityUserToken") ||
                   tipo.Name.StartsWith("IdentityUserClaim") ||
                   tipo.Name.StartsWith("IdentityUserLogin") ||
                   tipo.Name.StartsWith("IdentityRoleClaim") ||
                   tipo.Name.StartsWith("IdentityUserRole");
        }
    }
}