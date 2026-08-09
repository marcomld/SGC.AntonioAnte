using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Commands
{
    // 1. COMMAND (record posicional e inmutable)
    public record AsignarRolUsuarioCommand(Guid UsuarioId, string NombreRol) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class AsignarRolUsuarioCommandValidator : AbstractValidator<AsignarRolUsuarioCommand>
    {
        public AsignarRolUsuarioCommandValidator()
        {
            RuleFor(v => v.UsuarioId)
                .NotEmpty().WithMessage("El identificador del funcionario es obligatorio.");

            RuleFor(v => v.NombreRol)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.");
        }
    }

    // 3. HANDLER
    public class AsignarRolUsuarioCommandHandler : IRequestHandler<AsignarRolUsuarioCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AsignarRolUsuarioCommandHandler(
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

        public async Task<OperacionResultadoDto> Handle(AsignarRolUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null)
            {
                return OperacionResultadoDto.Fallo("Funcionario no encontrado.");
            }

            var existeRol = await _roleManager.RoleExistsAsync(request.NombreRol.Trim());
            if (!existeRol)
            {
                return OperacionResultadoDto.Fallo($"El rol '{request.NombreRol.Trim()}' no existe en la base de datos.");
            }

            var result = await _userManager.AddToRoleAsync(usuario, request.NombreRol.Trim());
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo($"El funcionario ya cuenta con el rol '{request.NombreRol.Trim()}' o no se pudo asignar.");
            }

            // 🔹 Registro explícito de auditoría para asignación de roles
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ASIGNAR_ROL_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Rol '{request.NombreRol.Trim()}' asignado al funcionario {usuario.Nombres} {usuario.Apellidos}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"Rol '{request.NombreRol.Trim()}' asignado correctamente al funcionario.");
        }
    }
}