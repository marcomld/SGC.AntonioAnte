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
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", "Seguridad");
            builder.Property(u => u.Identificacion).HasMaxLength(20).IsRequired();
            builder.Property(u => u.Nombres).HasMaxLength(150).IsRequired();
            builder.Property(u => u.Apellidos).HasMaxLength(150).IsRequired();
            builder.Property(u => u.FechaCreacion).IsRequired();

            // 🔹 Nuevo: Configuración de la Relación Usuario -> Departamento
            builder.HasOne(u => u.Departamento)
                   .WithMany(d => d.Usuarios)
                   .HasForeignKey(u => u.DepartamentoId)
                   .OnDelete(DeleteBehavior.SetNull); // Si se borra un departamento, los usuarios quedan en NULL, no se borran.
        }
    }
}
