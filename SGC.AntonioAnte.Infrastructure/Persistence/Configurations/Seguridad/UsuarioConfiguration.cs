using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Configurations.Seguridad
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", "Seguridad");
            builder.Property(u => u.Identificacion).HasMaxLength(20).IsRequired();
            builder.Property(u => u.Nombres).HasMaxLength(150).IsRequired();
            builder.Property(u => u.Apellidos).HasMaxLength(150).IsRequired();
            builder.Property(u => u.Departamento).HasMaxLength(100);
            builder.Property(u => u.FechaCreacion).IsRequired();
        }
    }
}
