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

            // Escanea y aplica automáticamente TODAS las configuraciones IEntityTypeConfiguration<T>
            // presentes en la capa Infrastructure
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Interceptamos únicamente las entidades en estado "Modificado" ignorando la propia tabla Auditoria
            var entidadesModificadas = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified && e.Entity is not Auditoria)
                .ToList();

            var nuevasAuditorias = new List<Auditoria>();

            // Propiedades de Identity/Técnicas que no queremos registrar en la bitácora
            var propiedadesOmitidas = new[] { "ConcurrencyStamp", "SecurityStamp", "NormalizedEmail", "NormalizedUserName", "PasswordHash" };

            foreach (var entry in entidadesModificadas)
            {
                var cambios = new List<string>();

                foreach (var propiedad in entry.Properties)
                {
                    if (propiedad.IsModified && !propiedadesOmitidas.Contains(propiedad.Metadata.Name))
                    {
                        var valorAnterior = propiedad.OriginalValue ?? "null";
                        var valorNuevo = propiedad.CurrentValue ?? "null";

                        // Solo registramos si el valor en texto realmente cambió
                        if (valorAnterior.ToString() != valorNuevo.ToString())
                        {
                            cambios.Add($"{propiedad.Metadata.Name}: '{valorAnterior}' -> '{valorNuevo}'");
                        }
                    }
                }

                if (cambios.Any())
                {
                    string nombreEntidad = entry.Entity.GetType().Name;

                    // Extraemos la propiedad "Id" dinámicamente sin importar el tipo de entidad
                    var idPropiedad = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue?.ToString() ?? "N/A";

                    nuevasAuditorias.Add(new Auditoria
                    {
                        UsuarioId = _currentUserService.UsuarioIdGuid,
                        Accion = $"ACTUALIZAR_{nombreEntidad.ToUpper()}",
                        Entidad = nombreEntidad,
                        EntidadId = idPropiedad,
                        DatosAdicionales = $"Cambios aplicados: {string.Join(" | ", cambios)}",
                        DireccionIp = _currentUserService.IpAddress,
                        Navegador = _currentUserService.UserAgent,
                        FechaCreacion = DateTime.UtcNow
                    });
                }
            }

            // Agregamos las auditorías acumuladas al contexto antes de enviar la transacción a la BD
            if (nuevasAuditorias.Any())
            {
                Auditorias.AddRange(nuevasAuditorias);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}