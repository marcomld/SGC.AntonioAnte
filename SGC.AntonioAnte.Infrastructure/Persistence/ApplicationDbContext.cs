using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Common.Attributes;
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

        // Módulo 1: Seguridad
        public DbSet<Auditoria> Auditorias => Set<Auditoria>();
        public DbSet<Departamento> Departamentos => Set<Departamento>();

        // Módulo 2: Catastro
        public DbSet<TipoTenencia> TiposTenencia => Set<TipoTenencia>();
        public DbSet<TipoEstructura> TiposEstructura => Set<TipoEstructura>();
        public DbSet<EstadoConservacion> EstadosConservacion => Set<EstadoConservacion>();
        public DbSet<Propietario> Propietarios => Set<Propietario>();
        public DbSet<Predio> Predios => Set<Predio>();
        public DbSet<Dominio> Dominios => Set<Dominio>();
        public DbSet<BloqueConstruccion> BloquesConstruccion => Set<BloqueConstruccion>();

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
        // TRADUCCIÓN 100% GENÉRICA MEDIANTE REFLECTION Y ATRIBUTOS
        // =========================================================================
        private string FormatearCampoCreado(PropertyEntry propiedad, object entidad)
        {
            string valorLegible = ResolverValorLegible(propiedad, entidad, propiedad.CurrentValue);
            string nombreCampo = ObtenerEtiquetaCampo(propiedad);

            return $"{nombreCampo}: '{valorLegible}'";
        }

        private string FormatearCampoModificado(PropertyEntry propiedad, object entidad)
        {
            string valorAnteriorLegible = ResolverValorLegible(propiedad, entidad, propiedad.OriginalValue);
            string valorNuevoLegible = ResolverValorLegible(propiedad, entidad, propiedad.CurrentValue);
            string nombreCampo = ObtenerEtiquetaCampo(propiedad);

            return $"{nombreCampo}: '{valorAnteriorLegible}' -> '{valorNuevoLegible}'";
        }

        private string FormatearCampoEliminado(PropertyEntry propiedad, object entidad)
        {
            string valorLegible = ResolverValorLegible(propiedad, entidad, propiedad.OriginalValue);
            string nombreCampo = ObtenerEtiquetaCampo(propiedad);

            return $"{nombreCampo}: '{valorLegible}'";
        }

        /// <summary>
        /// Método genérico que mediante Reflection busca si la propiedad tiene el atributo [AuditDisplayName].
        /// Si lo tiene, extrae el texto legible de la entidad de navegación asociada.
        /// </summary>
        private string ResolverValorLegible(PropertyEntry propiedad, object entidad, object? valorRaw)
        {
            if (valorRaw == null || valorRaw.ToString() == "null") return "null";

            // 1. Buscar si la propiedad del objeto tiene el atributo [AuditDisplayName]
            var propInfo = entidad.GetType().GetProperty(propiedad.Metadata.Name);
            var auditAttr = propInfo?.GetCustomAttribute<AuditDisplayNameAttribute>();

            if (auditAttr != null)
            {
                // 2. Extraer la propiedad de navegación (ej. 'Departamento')
                var navProp = entidad.GetType().GetProperty(auditAttr.NavigationPropertyName);
                var objetoNavegacion = navProp?.GetValue(entidad);

                if (objetoNavegacion != null)
                {
                    // Extraer la propiedad de texto (ej. 'Nombre')
                    var displayProp = objetoNavegacion.GetType().GetProperty(auditAttr.DisplayPropertyName);
                    var textoLegible = displayProp?.GetValue(objetoNavegacion)?.ToString();

                    if (!string.IsNullOrWhiteSpace(textoLegible))
                        return textoLegible;
                }

                // 3. Si la propiedad de navegación no estaba cargada en memoria, consultar usando Find(Type, Key)
                var targetType = objetoNavegacion?.GetType() ?? navProp?.PropertyType;
                if (targetType != null && valorRaw is Guid guidId && guidId != Guid.Empty)
                {
                    // 🔹 Solución CS0411: Usamos la sobrecarga no genérica DbContext.Find(Type, key)
                    var objetoDb = Find(targetType, guidId);
                    if (objetoDb != null)
                    {
                        var displayProp = objetoDb.GetType().GetProperty(auditAttr.DisplayPropertyName);
                        return displayProp?.GetValue(objetoDb)?.ToString() ?? valorRaw.ToString()!;
                    }
                }
            }

            return valorRaw.ToString()!;
        }

        private static string ObtenerEtiquetaCampo(PropertyEntry propiedad)
        {
            if (propiedad.Metadata.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && propiedad.Metadata.Name.Length > 2)
            {
                return propiedad.Metadata.Name.Substring(0, propiedad.Metadata.Name.Length - 2);
            }
            return propiedad.Metadata.Name;
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