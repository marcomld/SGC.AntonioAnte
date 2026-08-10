using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands
{
    // 1. COMMAND (record posicional e inmutable)
    public record CambiarEstadoUsuarioCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class CambiarEstadoUsuarioCommandValidator : AbstractValidator<CambiarEstadoUsuarioCommand>
    {
        public CambiarEstadoUsuarioCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El identificador del funcionario es obligatorio.");
        }
    }

    // 3. HANDLER
    public class CambiarEstadoUsuarioCommandHandler : IRequestHandler<CambiarEstadoUsuarioCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;

        public CambiarEstadoUsuarioCommandHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.Id.ToString());
            if (usuario == null)
            {
                return OperacionResultadoDto.Fallo("Funcionario no encontrado.");
            }

            // Alternamos el estado (Toggle)
            usuario.EstadoActivo = !usuario.EstadoActivo;

            // _userManager.UpdateAsync ejecuta internamente SaveChangesAsync de DbContext,
            // disparando automáticamente nuestro interceptor de auditoría (ACTIVAR_USUARIO / DESACTIVAR_USUARIO)
            var result = await _userManager.UpdateAsync(usuario);
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo("No se pudo cambiar el estado del funcionario en la base de datos.");
            }

            string estadoTexto = usuario.EstadoActivo ? "activado" : "desactivado";
            return OperacionResultadoDto.Exito($"Acceso del funcionario '{usuario.Nombres} {usuario.Apellidos}' {estadoTexto} correctamente.");
        }
    }
}