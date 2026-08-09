using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Commands
{
    // 1. COMMAND (record posicional e inmutable)
    public record DeleteRolCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR
    public class DeleteRolCommandValidator : AbstractValidator<DeleteRolCommand>
    {
        public DeleteRolCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El identificador del rol es obligatorio.");
        }
    }

    // 3. HANDLER
    public class DeleteRolCommandHandler : IRequestHandler<DeleteRolCommand, OperacionResultadoDto>
    {
        private readonly RoleManager<Rol> _roleManager;

        public DeleteRolCommandHandler(RoleManager<Rol> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<OperacionResultadoDto> Handle(DeleteRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (rol == null)
            {
                return OperacionResultadoDto.Fallo("El rol especificado no existe.");
            }

            if (rol.Name == "AdminSistemas" || rol.Name == "SuperAdmin")
            {
                return OperacionResultadoDto.Fallo("No es posible eliminar los roles base del sistema.");
            }

            string nombreRol = rol.Name ?? string.Empty;

            var result = await _roleManager.DeleteAsync(rol);
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo("No se pudo eliminar el rol de la base de datos.");
            }

            return OperacionResultadoDto.Exito($"Rol '{nombreRol}' eliminado permanentemente.");
        }
    }
}