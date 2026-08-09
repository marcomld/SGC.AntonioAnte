using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Command
{
    // 1. COMMAND (record posicional e inmutable)
    public record UpdateRolCommand(Guid Id, string Nombre, string Descripcion) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class UpdateRolCommandValidator : AbstractValidator<UpdateRolCommand>
    {
        public UpdateRolCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El identificador del rol es obligatorio.");

            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre del rol no debe superar los 50 caracteres.");

            RuleFor(v => v.Descripcion)
                .NotEmpty().WithMessage("La descripción del rol es obligatoria.")
                .MaximumLength(250).WithMessage("La descripción no debe superar los 250 caracteres.");
        }
    }

    // 3. HANDLER
    public class UpdateRolCommandHandler : IRequestHandler<UpdateRolCommand, OperacionResultadoDto>
    {
        private readonly RoleManager<Rol> _roleManager;

        public UpdateRolCommandHandler(RoleManager<Rol> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<OperacionResultadoDto> Handle(UpdateRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (rol == null)
            {
                return OperacionResultadoDto.Fallo("El rol especificado no existe.");
            }

            // 🛡️ Protección de Roles Base
            if (rol.Name == "AdminSistemas" || rol.Name == "SuperAdmin")
            {
                return OperacionResultadoDto.Fallo("No está permitido modificar los roles base del sistema.");
            }

            var nombreTrim = request.Nombre.Trim();

            // 🛡️ Control de duplicados (excluyendo el rol actual)
            var rolConMismoNombre = await _roleManager.FindByNameAsync(nombreTrim);
            if (rolConMismoNombre != null && rolConMismoNombre.Id != request.Id)
            {
                return OperacionResultadoDto.Fallo($"Ya existe otro rol registrado con el nombre '{nombreTrim}'.");
            }

            rol.Name = nombreTrim;
            rol.NormalizedName = nombreTrim.ToUpper();
            rol.Descripcion = request.Descripcion.Trim();

            // 🔹 El DbContext intercepta el cambio y audita ACTUALIZAR_ROL de forma automática
            var result = await _roleManager.UpdateAsync(rol);
            if (!result.Succeeded)
            {
                return OperacionResultadoDto.Fallo("No se pudieron guardar los cambios del rol en la base de datos.");
            }

            return OperacionResultadoDto.Exito($"El rol '{nombreTrim}' fue actualizado correctamente.");
        }
    }
}
