using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Queries
{
    public class ObtenerPermisosRolQuery : IRequest<List<PermissionDto>>
    {
        public Guid RolId { get; set; }

        public ObtenerPermisosRolQuery() { }

        public ObtenerPermisosRolQuery(Guid rolId)
        {
            RolId = rolId;
        }
    }

    public class ObtenerPermisosRolQueryHandler : IRequestHandler<ObtenerPermisosRolQuery, List<PermissionDto>>
    {
        private readonly RoleManager<Rol> _roleManager;

        public ObtenerPermisosRolQueryHandler(RoleManager<Rol> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<PermissionDto>> Handle(ObtenerPermisosRolQuery request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.RolId.ToString());
            if (rol == null)
                throw new Exception("El rol especificado no existe.");

            // 1. Obtener catálogo maestro de permisos estático
            var catalogo = Permissions.ObtenerCatalogoMaestro();

            // 2. Obtener claims actuales guardados en la tabla RolClaims
            var claimsActualesRol = await _roleManager.GetClaimsAsync(rol);
            var valoresClaimsRol = claimsActualesRol.Select(c => c.Value).ToHashSet();

            // 3. Cruzar catálogo para marcar estado activo
            foreach (var permiso in catalogo)
            {
                if (valoresClaimsRol.Contains(permiso.ValorClaim))
                {
                    permiso.EstaActivo = true;
                }
            }

            return catalogo;
        }
    }
}
