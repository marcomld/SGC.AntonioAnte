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
    public class ObtenerPermisosUsuarioQuery : IRequest<List<PermissionDto>>
    {
        public Guid UsuarioId { get; set; }

        public ObtenerPermisosUsuarioQuery() { }

        public ObtenerPermisosUsuarioQuery(Guid usuarioId)
        {
            UsuarioId = usuarioId;
        }
    }

    public class ObtenerPermisosUsuarioQueryHandler : IRequestHandler<ObtenerPermisosUsuarioQuery, List<PermissionDto>>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;

        public ObtenerPermisosUsuarioQueryHandler(
            UserManager<Usuario> userManager,
            RoleManager<Rol> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<PermissionDto>> Handle(ObtenerPermisosUsuarioQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null)
                throw new Exception("El funcionario especificado no existe.");

            // 1. Obtener catálogo maestro estático
            var catalogo = Permissions.ObtenerCatalogoMaestro();

            // 2. Obtener claims heredados por los roles del usuario
            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            var claimsHeredadosRoles = new Dictionary<string, string>(); // Key: ValorClaim, Value: NombreRol

            foreach (var nombreRol in rolesUsuario)
            {
                var objetoRol = await _roleManager.FindByNameAsync(nombreRol);
                if (objetoRol != null)
                {
                    var claimsDelRol = await _roleManager.GetClaimsAsync(objetoRol);
                    foreach (var claim in claimsDelRol)
                    {
                        if (!claimsHeredadosRoles.ContainsKey(claim.Value))
                        {
                            claimsHeredadosRoles.Add(claim.Value, nombreRol);
                        }
                    }
                }
            }

            // 3. Obtener claims directos en la tabla UsuarioClaims
            var claimsDirectosUsuario = await _userManager.GetClaimsAsync(usuario);
            var valoresClaimsDirectos = claimsDirectosUsuario.Select(c => c.Value).ToHashSet();

            // 4. Cruzar el catálogo para la UI
            foreach (var permiso in catalogo)
            {
                // ¿Heredado por rol?
                if (claimsHeredadosRoles.TryGetValue(permiso.ValorClaim, out string? rolOrigen))
                {
                    permiso.EsHeredadoDeRol = true;
                    permiso.NombreRolOrigen = rolOrigen;
                    permiso.EstaActivo = true;
                }

                // ¿Asignado directamente al usuario?
                if (valoresClaimsDirectos.Contains(permiso.ValorClaim))
                {
                    permiso.EstaActivo = true;
                }
            }

            return catalogo;
        }
    }
}
