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
    public class BloqueConstruccionConfiguration : IEntityTypeConfiguration<BloqueConstruccion>
    {
        public void Configure(EntityTypeBuilder<BloqueConstruccion> builder)
        {
            builder.ToTable("BloquesConstruccion", "Catastro");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.NumeroBloque).IsRequired();
            builder.Property(b => b.NumeroPisos).IsRequired();
            builder.Property(b => b.AreaConstruccion).HasPrecision(18, 2).IsRequired();
            builder.Property(b => b.AnioConstruccion).IsRequired();

            // Relaciones
            builder.HasOne(b => b.TipoEstructura)
                .WithMany()
                .HasForeignKey(b => b.TipoEstructuraId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.EstadoConservacion)
                .WithMany()
                .HasForeignKey(b => b.EstadoConservacionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
