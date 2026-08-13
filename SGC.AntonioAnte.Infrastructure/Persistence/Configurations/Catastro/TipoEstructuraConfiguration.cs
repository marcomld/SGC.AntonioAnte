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
    public class TipoEstructuraConfiguration : IEntityTypeConfiguration<TipoEstructura>
    {
        public void Configure(EntityTypeBuilder<TipoEstructura> builder)
        {
            builder.ToTable("TiposEstructura", "Catastro");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(t => t.Descripcion).HasMaxLength(250);
        }
    }
}
