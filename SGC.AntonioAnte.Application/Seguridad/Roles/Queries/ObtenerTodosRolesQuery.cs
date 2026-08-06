using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Queries
{
    public record ObtenerTodosRolesQuery : IRequest<List<RoleResponseDto>>;

    public class ObtenerTodosRolesQueryHandler : IRequestHandler<ObtenerTodosRolesQuery, List<RoleResponseDto>>
    {
        private readonly RoleManager<Rol> _roleManager;
        private readonly UserManager<Usuario> _userManager;

        public ObtenerTodosRolesQueryHandler(RoleManager<Rol> roleManager, UserManager<Usuario> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<RoleResponseDto>> Handle(ObtenerTodosRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);
            var resultado = new List<RoleResponseDto>();

            foreach (var rol in roles)
            {
                // Conteo de usuarios utilizando Identity sin requerir IApplicationDbContext
                var usuariosEnRol = await _userManager.GetUsersInRoleAsync(rol.Name ?? string.Empty);

                resultado.Add(new RoleResponseDto
                {
                    Id = rol.Id,
                    Nombre = rol.Name ?? string.Empty,
                    Descripcion = rol.Descripcion ?? "Sin descripción asignada",
                    CantidadUsuarios = usuariosEnRol.Count
                });
            }

            return resultado;
        }
    }
}
