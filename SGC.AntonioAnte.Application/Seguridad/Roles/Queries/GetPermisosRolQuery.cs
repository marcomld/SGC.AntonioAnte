using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Queries
{
    // 1. QUERY (record posicional)
    public record GetPermisosRolQuery(Guid Id) : IRequest<List<PermissionDto>>;

    // 2. HANDLER
    public class GetPermisosRolQueryHandler : IRequestHandler<GetPermisosRolQuery, List<PermissionDto>>
    {
        private readonly RoleManager<Rol> _roleManager;

        public GetPermisosRolQueryHandler(RoleManager<Rol> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<PermissionDto>> Handle(GetPermisosRolQuery request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (rol == null)
            {
                return new List<PermissionDto>();
            }

            // 1. Catálogo maestro de permisos estático
            var catalogo = Permissions.ObtenerCatalogoMaestro();

            // 2. Claims actuales guardados en RolClaims
            var claimsActualesRol = await _roleManager.GetClaimsAsync(rol);
            var valoresClaimsRol = claimsActualesRol.Select(c => c.Value).ToHashSet();

            // 3. Cruzar con el catálogo
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