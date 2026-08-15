using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Configurations.Catastro
{
    public class PropietarioConfiguration : IEntityTypeConfiguration<Propietario>
    {
        public void Configure(EntityTypeBuilder<Propietario> builder)
        {
            builder.ToTable("Propietarios", "Catastro");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.TipoPropietario).IsRequired();

            // Índice único para evitar duplicar ciudadanos o empresas por Cédula/RUC
            builder.Property(p => p.Identificacion).HasMaxLength(20).IsRequired();
            builder.HasIndex(p => p.Identificacion).IsUnique();

            builder.Property(p => p.Nombres).HasMaxLength(100);
            builder.Property(p => p.Apellidos).HasMaxLength(100);
            builder.Property(p => p.RazonSocial).HasMaxLength(200);
            builder.Property(p => p.Email).HasMaxLength(100);
            builder.Property(p => p.Telefono).HasMaxLength(20);
        }
    }
}
