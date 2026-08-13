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
    public class DominioConfiguration : IEntityTypeConfiguration<Dominio>
    {
        public void Configure(EntityTypeBuilder<Dominio> builder)
        {
            builder.ToTable("Dominios", "Catastro");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.PorcentajePropiedad).HasPrecision(5, 2).IsRequired();
            builder.Property(d => d.Notaria).HasMaxLength(100);

            // Relaciones con comportamiento Restrict para proteger la integridad histórica de la propiedad
            builder.HasOne(d => d.Propietario)
                .WithMany(p => p.Dominios)
                .HasForeignKey(d => d.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.TipoTenencia)
                .WithMany()
                .HasForeignKey(d => d.TipoTenenciaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
