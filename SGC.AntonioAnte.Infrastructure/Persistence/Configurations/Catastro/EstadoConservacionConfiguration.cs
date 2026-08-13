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
    public class EstadoConservacionConfiguration : IEntityTypeConfiguration<EstadoConservacion>
    {
        public void Configure(EntityTypeBuilder<EstadoConservacion> builder)
        {
            builder.ToTable("EstadosConservacion", "Catastro");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Descripcion).HasMaxLength(250);
        }
    }
}
