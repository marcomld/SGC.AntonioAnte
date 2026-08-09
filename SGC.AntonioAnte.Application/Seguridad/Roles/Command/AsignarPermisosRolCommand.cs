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
    public record AsignarPermisosRolCommand(Guid RolId, List<PermissionDto> Permisos) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR
    public class AsignarPermisosRolCommandValidator : AbstractValidator<AsignarPermisosRolCommand>
    {
        public AsignarPermisosRolCommandValidator()
        {
            RuleFor(v => v.RolId)
                .NotEmpty().WithMessage("El identificador del rol es obligatorio.");

            RuleFor(v => v.Permisos)
                .NotNull().WithMessage("La lista de permisos no puede ser nula.");
        }
    }

    // 3. HANDLER
    public class AsignarPermisosRolCommandHandler : IRequestHandler<AsignarPermisosRolCommand, OperacionResultadoDto>
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

        public async Task<OperacionResultadoDto> Handle(AsignarPermisosRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.RolId.ToString());
            if (rol == null)
            {
                return OperacionResultadoDto.Fallo("El rol especificado no existe.");
            }

            // 1. Obtener claims actuales en base de datos
            var claimsActuales = await _roleManager.GetClaimsAsync(rol);
            var oldClaimsSet = claimsActuales.Select(c => c.Value).ToHashSet();

            // 2. Obtener claims solicitados desde la interfaz UI
            var newClaimsSet = request.Permisos?
                .Select(p => p.ValorClaim)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToHashSet() ?? new HashSet<string>();

            // 3. 🎯 Cálculo de Delta (Diferencias exactas)
            var agregados = newClaimsSet.Except(oldClaimsSet).ToList();
            var removidos = oldClaimsSet.Except(newClaimsSet).ToList();

            // Si no cambió absolutamente nada, evitamos escrituras innecesarias
            if (!agregados.Any() && !removidos.Any())
            {
                return OperacionResultadoDto.Exito($"No se detectaron cambios en los permisos del rol '{rol.Name}'.");
            }

            // 4. Reemplazar claims en la tabla RolClaims
            foreach (var claim in claimsActuales)
            {
                await _roleManager.RemoveClaimAsync(rol, claim);
            }

            foreach (var valClaim in newClaimsSet)
            {
                await _roleManager.AddClaimAsync(rol, new Claim(Permissions.ClaimType, valClaim));
            }

            // 5. Formatear mensaje detallado para la bitácora
            var detallesAuditoria = new List<string>();
            if (agregados.Any())
                detallesAuditoria.Add($"Agregados (+{agregados.Count}): [{string.Join(", ", agregados)}]");

            if (removidos.Any())
                detallesAuditoria.Add($"Removidos (-{removidos.Count}): [{string.Join(", ", removidos)}]");

            string datosAdicionales = $"Modificación en rol '{rol.Name}': {string.Join(" | ", detallesAuditoria)}";

            // 6. Guardar auditoría
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ACTUALIZAR_PERMISOS_ROL",
                Entidad = "Rol",
                EntidadId = rol.Id.ToString(),
                DatosAdicionales = datosAdicionales,
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return OperacionResultadoDto.Exito($"Matriz de permisos del rol '{rol.Name}' actualizada correctamente.");
        }
    }
}