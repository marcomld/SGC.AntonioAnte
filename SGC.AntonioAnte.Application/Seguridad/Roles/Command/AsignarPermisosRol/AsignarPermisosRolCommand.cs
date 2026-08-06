using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Command.AsignarPermisosRol
{
    // COMMAND
    public class AsignarPermisosRolCommand : IRequest<bool>
    {
        public Guid RolId { get; set; }
        public List<PermissionDto> Permisos { get; set; } = new();
    }

    // HANDLER
    public class AsignarPermisosRolCommandHandler : IRequestHandler<AsignarPermisosRolCommand, bool>
    {
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AsignarPermisosRolCommandHandler(
            RoleManager<Rol> roleManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _roleManager = roleManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AsignarPermisosRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.RolId.ToString());
            if (rol == null)
                throw new Exception("El rol especificado no existe.");

            // 1. Limpiar claims anteriores del rol
            var claimsActuales = await _roleManager.GetClaimsAsync(rol);
            foreach (var claim in claimsActuales)
            {
                await _roleManager.RemoveClaimAsync(rol, claim);
            }

            // 2. Asignar los nuevos claims seleccionados
            int cantidadAsignada = 0;
            foreach (var perm in request.Permisos)
            {
                var nuevoClaim = new Claim(Permissions.ClaimType, perm.ValorClaim);
                var resAdd = await _roleManager.AddClaimAsync(rol, nuevoClaim);
                if (resAdd.Succeeded) cantidadAsignada++;
            }

            // 3. Auditoría de cambios
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ACTUALIZAR_PERMISOS_ROL",
                Entidad = "Rol",
                EntidadId = rol.Id.ToString(),
                DatosAdicionales = $"Se actualizaron los permisos del rol '{rol.Name}'. Total asignados: {cantidadAsignada}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
