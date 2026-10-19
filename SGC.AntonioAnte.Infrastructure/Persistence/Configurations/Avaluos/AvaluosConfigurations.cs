using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGC.AntonioAnte.Domain.Avaluos.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Configurations.Avaluos
{
    public class PrecioBaseSueloConfiguration : IEntityTypeConfiguration<PrecioBaseSuelo>
    {
        public void Configure(EntityTypeBuilder<PrecioBaseSuelo> builder)
        {
            builder.ToTable("PreciosBaseSuelo", "Avaluos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ValorMetroCuadrado).HasPrecision(18, 2);
            builder.Property(x => x.Zona).HasMaxLength(10);
            builder.Property(x => x.Sector).HasMaxLength(10);
            builder.Property(x => x.ManzanaPoligono).HasMaxLength(20);
        }
    }

    public class CostoReposicionConfiguration : IEntityTypeConfiguration<CostoReposicion>
    {
        public void Configure(EntityTypeBuilder<CostoReposicion> builder)
        {
            builder.ToTable("CostosReposicion", "Avaluos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ValorMetroCuadrado).HasPrecision(18, 2);

            // 🔹 Restricción FK
            builder.HasOne(x => x.TipoEstructura)
                   .WithMany()
                   .HasForeignKey(x => x.TipoEstructuraId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class FactorDepreciacionConfiguration : IEntityTypeConfiguration<FactorDepreciacion>
    {
        public void Configure(EntityTypeBuilder<FactorDepreciacion> builder)
        {
            builder.ToTable("FactoresDepreciacion", "Avaluos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Coeficiente).HasPrecision(5, 4);

            // 🔹 Restricción FK
            builder.HasOne(x => x.EstadoConservacion)
                   .WithMany()
                   .HasForeignKey(x => x.EstadoConservacionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class FactorAfectacionConfiguration : IEntityTypeConfiguration<FactorAfectacion>
    {
        public void Configure(EntityTypeBuilder<FactorAfectacion> builder)
        {
            builder.ToTable("FactoresAfectacion", "Avaluos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TipoAfectacion).HasMaxLength(100);
            builder.Property(x => x.Coeficiente).HasPrecision(5, 4);
        }
    }

    public class LiquidacionPredialConfiguration : IEntityTypeConfiguration<LiquidacionPredial>
    {
        public void Configure(EntityTypeBuilder<LiquidacionPredial> builder)
        {
            builder.ToTable("LiquidacionesPrediales", "Avaluos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ValorTerreno).HasPrecision(18, 2);
            builder.Property(x => x.ValorConstruccion).HasPrecision(18, 2);
            builder.Property(x => x.BaseImponible).HasPrecision(18, 2);
            builder.Property(x => x.ExoneracionesPrevias).HasPrecision(18, 2);
            builder.Property(x => x.ValorEmision).HasPrecision(18, 2);
            builder.Property(x => x.CemAplicada).HasPrecision(18, 2);
            builder.Property(x => x.EstadoLiquidacion).HasMaxLength(20);

            // 🔹 Restricción FK al Aggregate Root
            builder.HasOne(x => x.Predio)
                   .WithMany()
                   .HasForeignKey(x => x.PredioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
