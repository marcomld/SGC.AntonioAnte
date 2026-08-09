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
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Commands
{
    // 1. COMMAND
    public record AsignarPermisosGranularesCommand(Guid UsuarioId, List<PermissionDto> Permisos) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR
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

            // 1. Obtener todos los claims que el usuario YA hereda por sus roles asignados
            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            var claimsHeredadosRoles = new HashSet<string>();

            foreach (var nombreRol in rolesUsuario)
            {
                var rol = await _roleManager.FindByNameAsync(nombreRol);
                if (rol != null)
                {
                    var claimsRol = await _roleManager.GetClaimsAsync(rol);
                    foreach (var c in claimsRol)
                    {
                        claimsHeredadosRoles.Add(c.Value);
                    }
                }
            }

            // 2. Obtener los claims directos (excepciones) actuales en UsuarioClaims
            var claimsDirectosActuales = await _userManager.GetClaimsAsync(usuario);
            var oldDirectClaimsSet = claimsDirectosActuales.Select(c => c.Value).ToHashSet();

            // 3. Filtrar los nuevos claims directos (OMITIENDO los que ya vienen heredados por rol)
            var newDirectClaimsSet = request.Permisos?
                .Select(p => p.ValorClaim)
                .Where(v => !string.IsNullOrWhiteSpace(v) && !claimsHeredadosRoles.Contains(v))
                .ToHashSet() ?? new HashSet<string>();

            // 4. 🎯 Cálculo de Delta (Diferencias de permisos directos)
            var agregados = newDirectClaimsSet.Except(oldDirectClaimsSet).ToList();
            var removidos = oldDirectClaimsSet.Except(newDirectClaimsSet).ToList();

            if (!agregados.Any() && !removidos.Any())
            {
                return OperacionResultadoDto.Exito($"No se detectaron cambios en las excepciones de permisos del funcionario '{usuario.Nombres} {usuario.Apellidos}'.");
            }

            // 5. Reemplazar la tabla UsuarioClaims para este usuario
            if (claimsDirectosActuales.Count > 0)
            {
                await _userManager.RemoveClaimsAsync(usuario, claimsDirectosActuales);
            }

            foreach (var valClaim in newDirectClaimsSet)
            {
                await _userManager.AddClaimAsync(usuario, new Claim(Permissions.ClaimType, valClaim));
            }

            // 6. Formatear mensaje detallado de auditoría
            var detallesAuditoria = new List<string>();
            if (agregados.Any())
                detallesAuditoria.Add($"Agregados (+{agregados.Count}): [{string.Join(", ", agregados)}]");

            if (removidos.Any())
                detallesAuditoria.Add($"Removidos (-{removidos.Count}): [{string.Join(", ", removidos)}]");

            string datosAdicionales = $"Permisos especiales de '{usuario.Nombres} {usuario.Apellidos}' modificadas: {string.Join(" | ", detallesAuditoria)}";

            // 7. Guardar auditoría
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ACTUALIZAR_PERMISOS_DIRECTOS_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = datosAdicionales,
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return OperacionResultadoDto.Exito($"Permisos especiales actualizados correctamente para '{usuario.Nombres} {usuario.Apellidos}'.");
        }
    }
}