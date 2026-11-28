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
    public class RecuperacionPasswordConfiguration
        : IEntityTypeConfiguration<RecuperacionPassword>
    {
        public void Configure(
            EntityTypeBuilder<RecuperacionPassword> builder)
        {
            builder.ToTable("RecuperacionesPassword", "Seguridad");

            builder.HasKey(x => x.UsuarioId);

            builder.HasOne(x => x.Usuario)
                .WithOne()
                .HasForeignKey<RecuperacionPassword>(
                    x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.CodigoOtpProtegido)
                .HasMaxLength(1024);

            builder.Property(x => x.ResetTokenHash)
                .HasMaxLength(64);

            builder.Property(x => x.SecurityStamp)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.IntentosFallidos)
                .HasDefaultValue(0);

            builder.Property(x => x.RowVersion)
                .IsRowVersion();
        }
    }
}
