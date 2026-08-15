using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Configurations.Catastro
{
    public class PredioConfiguration : IEntityTypeConfiguration<Predio>
    {
        public void Configure(EntityTypeBuilder<Predio> builder)
        {
            builder.ToTable("Predios", "Catastro");
            builder.HasKey(p => p.Id);

            // Mapeo del Value Object ClaveCatastral
            builder.Property(p => p.ClaveCatastral)
                .HasConversion(
                    clave => clave.Valor,
                    valor => ClaveCatastral.Crear(valor))
                .HasColumnName("ClaveCatastral")
                .HasMaxLength(28)
                .IsRequired();

            // Índice único para garantizar que no existan predios duplicados con la misma clave MIDUVI
            builder.HasIndex(p => p.ClaveCatastral).IsUnique();

            builder.Property(p => p.ClaveAnterior).HasMaxLength(50);
            builder.Property(p => p.TipoPredio).IsRequired();

            builder.Property(p => p.AreaTerrenoEscritura).HasPrecision(18, 2).IsRequired();
            builder.Property(p => p.AreaTerrenoGrafica).HasPrecision(18, 2).IsRequired();

            // Mapeo espacial NetTopologySuite hacia SQL Server Geometry
            builder.Property(p => p.PoligonoEspacial)
                .HasColumnType("geometry");

            builder.Property(p => p.Direccion).HasMaxLength(250).IsRequired();

            // Relaciones
            builder.HasMany(p => p.Dominios)
                .WithOne(d => d.Predio)
                .HasForeignKey(d => d.PredioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Bloques)
                .WithOne(b => b.Predio)
                .HasForeignKey(b => b.PredioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
