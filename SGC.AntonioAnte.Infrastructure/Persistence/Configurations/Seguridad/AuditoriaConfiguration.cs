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
    public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
    {
        public void Configure(EntityTypeBuilder<Auditoria> builder)
        {
            builder.ToTable("Auditorias", "Seguridad");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Accion).HasMaxLength(100).IsRequired();
            builder.Property(a => a.Entidad).HasMaxLength(100).IsRequired();

            // 🔹 Nuevo: Configuración de la Relación Auditoria -> Usuario (Candado de Seguridad)
            builder.HasOne(a => a.Usuario)
                   .WithMany(u => u.Auditorias)
                   .HasForeignKey(a => a.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict); // ¡Protege los registros históricos!
        }
    }
}
