using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Configurations.Seguridad
{
    public class IdentityTablesConfiguration :
        IEntityTypeConfiguration<IdentityUserRole<Guid>>,
        IEntityTypeConfiguration<IdentityUserClaim<Guid>>,
        IEntityTypeConfiguration<IdentityUserLogin<Guid>>,
        IEntityTypeConfiguration<IdentityRoleClaim<Guid>>,
        IEntityTypeConfiguration<IdentityUserToken<Guid>>
    {
        private const string Schema = "Seguridad";

        public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder) => builder.ToTable("UsuarioRoles", Schema);
        public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder) => builder.ToTable("UsuarioClaims", Schema);
        public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder) => builder.ToTable("UsuarioLogins", Schema);
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder) => builder.ToTable("RolClaims", Schema);
        public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder) => builder.ToTable("UsuarioTokens", Schema);
    }
}
