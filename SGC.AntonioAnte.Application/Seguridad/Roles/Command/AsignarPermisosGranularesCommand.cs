using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Command
{
    // 1. COMMAND (record posicional)
    public record AsignarPermisosGranularesCommand(Guid UsuarioId, List<PermissionDto> Permisos) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class AsignarPermisosGranularesCommandValidator : AbstractValidator<AsignarPermisosGranularesCommand>
    {
        public AsignarPermisosGranularesCommandValidator()
        {
            RuleFor(v => v.UsuarioId)
                .NotEmpty().WithMessage("El identificador del funcionario es obligatorio.");

            RuleFor(v => v.Permisos)
                .NotNull().WithMessage("La lista de permisos no puede ser nula.");
        }
    }

    // 3. HANDLER
    public class AsignarPermisosGranularesCommandHandler : IRequestHandler<AsignarPermisosGranularesCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AsignarPermisosGranularesCommandHandler(
            UserManager<Usuario> userManager,
            RoleManager<Rol> roleManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<OperacionResultadoDto> Handle(AsignarPermisosGranularesCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null)
            {
                return OperacionResultadoDto.Fallo("El funcionario especificado no existe.");
            }

            // 1. Obtener todos los claims que el usuario YA hereda por sus roles
            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            var claimsHeredados = new HashSet<string>();

            foreach (var nombreRol in rolesUsuario)
            {
                var rol = await _roleManager.FindByNameAsync(nombreRol);
                if (rol != null)
                {
                    var claimsRol = await _roleManager.GetClaimsAsync(rol);
                    foreach (var c in claimsRol)
                    {
                        claimsHeredados.Add(c.Value);
                    }
                }
            }

            // 2. Limpiar la tabla UsuarioClaims para este usuario
            var claimsDirectosActuales = await _userManager.GetClaimsAsync(usuario);
            if (claimsDirectosActuales.Count > 0)
            {
                await _userManager.RemoveClaimsAsync(usuario, claimsDirectosActuales);
            }

            // 3. Insertar ÚNICAMENTE los permisos directos que NO están en los roles del usuario
            int cantidadExcepciones = 0;
            if (request.Permisos != null)
            {
                foreach (var perm in request.Permisos)
                {
                    if (claimsHeredados.Contains(perm.ValorClaim))
                        continue;

                    var nuevoClaim = new Claim(Permissions.ClaimType, perm.ValorClaim);
                    var resAdd = await _userManager.AddClaimAsync(usuario, nuevoClaim);
                    if (resAdd.Succeeded) cantidadExcepciones++;
                }
            }

            // 4. Auditoría de cambios
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ACTUALIZAR_PERMISOS_DIRECTOS_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Se asignaron {cantidadExcepciones} permisos granulares especiales directos para '{usuario.Nombres} {usuario.Apellidos}'.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return OperacionResultadoDto.Exito($"Permisos especiales actualizados correctamente para '{usuario.Nombres} {usuario.Apellidos}'.");
        }
    }
}