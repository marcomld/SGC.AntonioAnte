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
    public class DepartamentoConfiguration : IEntityTypeConfiguration<Departamento>
    {
        public void Configure(EntityTypeBuilder<Departamento> builder)
        {
            builder.ToTable("Departamentos", "Seguridad"); // Schema "Seguridad"

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Descripcion)
                .HasMaxLength(250);

            builder.Property(d => d.EstadoActivo)
                .HasDefaultValue(true);
        }
    }
}
