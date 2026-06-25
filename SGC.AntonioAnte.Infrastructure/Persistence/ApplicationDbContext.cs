using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence
{

    // Pasamos explícitamente Usuario, Rol y Guid
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Auditoria> Auditorias { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            string schema = "Seguridad";

            // 1. Configuración de Seguridad.Usuarios
            builder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios", schema);

                entity.Property(e => e.Identificacion).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Nombres).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Apellidos).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Departamento).HasMaxLength(100);
                entity.Property(e => e.FechaCreacion).IsRequired();
            });

            // 2. Configuración de Seguridad.Roles
            builder.Entity<Rol>(entity =>
            {
                entity.ToTable("Roles", schema);

                entity.Property(e => e.Descripcion).HasMaxLength(250);
            });

            // 3. Renombrar tablas intermedias y auxiliares
            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UsuarioRoles", schema);
            });

            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UsuarioClaims", schema);
            });

            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UsuarioLogins", schema);
            });

            builder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("RolClaims", schema);
            });

            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UsuarioTokens", schema);
            });

            builder.Entity<Auditoria>(entity =>
            {
                entity.ToTable("Auditorias", schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Accion).HasMaxLength(50);
                entity.Property(e => e.Entidad).HasMaxLength(100);
            });
        }
    }
}
