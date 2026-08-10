using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands
{
    // 1. COMMAND (record posicional e inmutable)
    public record AsignarPermisoUsuarioCommand(
        Guid UsuarioId,
        string TipoClaim,
        string ValorClaim
    ) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class AsignarPermisoUsuarioCommandValidator : AbstractValidator<AsignarPermisoUsuarioCommand>
    {
        public AsignarPermisoUsuarioCommandValidator()
        {
            RuleFor(v => v.UsuarioId)
                .NotEmpty().WithMessage("El identificador del funcionario es obligatorio.");

            RuleFor(v => v.TipoClaim)
                .NotEmpty().WithMessage("El tipo de permiso (TipoClaim) es obligatorio.");

            RuleFor(v => v.ValorClaim)
                .NotEmpty().WithMessage("El valor del permiso (ValorClaim) es obligatorio.");
        }
    }

    // 3. HANDLER
    public class AsignarPermisoUsuarioCommandHandler : IRequestHandler<AsignarPermisoUsuarioCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AsignarPermisoUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<OperacionResultadoDto> Handle(AsignarPermisoUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null)
            {
                return OperacionResultadoDto.Fallo("El funcionario especificado no existe.");
            }

            var tipoNorm = request.TipoClaim.Trim();
            var valorNorm = request.ValorClaim.Trim();
            var nuevoClaim = new Claim(tipoNorm, valorNorm);

            // Persistir directamente en la tabla Seguridad.UsuarioClaims
            var result = await _userManager.AddClaimAsync(usuario, nuevoClaim);
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo("No se pudo asignar el permiso granular al funcionario.");
            }

            // Auditoría explícita
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ASIGNAR_PERMISO_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Permiso '{valorNorm}' ({tipoNorm}) asignado al funcionario {usuario.Nombres} {usuario.Apellidos}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"Permiso '{valorNorm}' asignado exitosamente al funcionario '{usuario.Nombres} {usuario.Apellidos}'.");
        }
    }
}