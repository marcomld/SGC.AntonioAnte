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
    // 1. COMMAND
    public class CreateRolCommand : IRequest<OperacionResultadoDto>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    // 2. VALIDATOR (FluentValidation)
    public class CreateRolCommandValidator : AbstractValidator<CreateRolCommand>
    {
        public CreateRolCommandValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre del rol no debe superar los 50 caracteres.");

            RuleFor(v => v.Descripcion)
                .NotEmpty().WithMessage("La descripción del rol es obligatoria.")
                .MaximumLength(250).WithMessage("La descripción no debe superar los 250 caracteres.");
        }
    }

    // 3. HANDLER
    public class CreateRolCommandHandler : IRequestHandler<CreateRolCommand, OperacionResultadoDto>
    {
        private readonly RoleManager<Rol> _roleManager;

        public CreateRolCommandHandler(RoleManager<Rol> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<OperacionResultadoDto> Handle(CreateRolCommand request, CancellationToken cancellationToken)
        {
            var nombreTrim = request.Nombre.Trim();

            // 🛡️ Control de duplicados
            var rolExistente = await _roleManager.RoleExistsAsync(nombreTrim);
            if (rolExistente)
            {
                return OperacionResultadoDto.Fallo($"El rol '{nombreTrim}' ya se encuentra registrado.");
            }

            var nuevoRol = new Rol
            {
                Id = Guid.NewGuid(),
                Name = nombreTrim,
                NormalizedName = nombreTrim.ToUpper(),
                Descripcion = request.Descripcion.Trim()
            };

            var result = await _roleManager.CreateAsync(nuevoRol);
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo("No se pudo registrar el nuevo rol en el sistema.");
            }

            // 🔹 La auditoría CREAR_ROL la genera automáticamente ApplicationDbContext
            return OperacionResultadoDto.Exito("Rol registrado exitosamente.");
        }
    }
}