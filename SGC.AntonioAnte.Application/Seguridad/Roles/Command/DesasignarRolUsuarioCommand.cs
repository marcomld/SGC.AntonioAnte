using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Command
{
    // 1. COMMAND (record posicional e inmutable)
    public record DesasignarRolUsuarioCommand(Guid UsuarioId, string NombreRol) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class DesasignarRolUsuarioCommandValidator : AbstractValidator<DesasignarRolUsuarioCommand>
    {
        public DesasignarRolUsuarioCommandValidator()
        {
            RuleFor(v => v.UsuarioId)
                .NotEmpty().WithMessage("El identificador del funcionario es obligatorio.");

            RuleFor(v => v.NombreRol)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.");
        }
    }

    // 3. HANDLER
    public class DesasignarRolUsuarioCommandHandler : IRequestHandler<DesasignarRolUsuarioCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DesasignarRolUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<OperacionResultadoDto> Handle(DesasignarRolUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null)
            {
                return OperacionResultadoDto.Fallo("Funcionario no encontrado.");
            }

            var result = await _userManager.RemoveFromRoleAsync(usuario, request.NombreRol.Trim());
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo($"No se pudo remover el rol '{request.NombreRol.Trim()}' del funcionario.");
            }

            // 🔹 Registro explícito de auditoría para desasignación de roles
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "DESASIGNAR_ROL_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Rol '{request.NombreRol.Trim()}' removido del funcionario {usuario.Nombres} {usuario.Apellidos}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"Rol '{request.NombreRol.Trim()}' removido correctamente del funcionario.");
        }
    }
}