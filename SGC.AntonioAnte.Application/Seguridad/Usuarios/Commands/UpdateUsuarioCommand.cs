using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands
{
    // 1. COMMAND (record posicional e inmutable)
    public record UpdateUsuarioCommand(
        Guid Id,
        string Nombres,
        string Apellidos,
        string Email,
        Guid? DepartamentoId
    ) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class UpdateUsuarioCommandValidator : AbstractValidator<UpdateUsuarioCommand>
    {
        public UpdateUsuarioCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El identificador del funcionario es obligatorio.");

            RuleFor(x => x.Nombres)
                .NotEmpty().WithMessage("Los nombres del funcionario son obligatorios.")
                .MaximumLength(150).WithMessage("Los nombres no pueden exceder los 150 caracteres.");

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Los apellidos del funcionario son obligatorios.")
                .MaximumLength(150).WithMessage("Los apellidos no pueden exceder los 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(x => x.DepartamentoId)
                .NotEmpty().WithMessage("El departamento es obligatorio.");
        }
    }

    // 3. HANDLER
    public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;

        public UpdateUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.Id.ToString());
            if (usuario == null)
            {
                return OperacionResultadoDto.Fallo("El funcionario especificado no existe en el sistema.");
            }

            // 1. Cargar la entidad Departamento para enlazar la propiedad de navegación
            Departamento? departamentoObj = null;
            if (request.DepartamentoId.HasValue && request.DepartamentoId != Guid.Empty)
            {
                departamentoObj = await _context.Departamentos
                    .FirstOrDefaultAsync(d => d.Id == request.DepartamentoId.Value, cancellationToken);

                if (departamentoObj == null)
                {
                    return OperacionResultadoDto.Fallo("El departamento municipal seleccionado no existe.");
                }
            }

            // 2. Asignar los nuevos valores
            usuario.Nombres = request.Nombres.Trim();
            usuario.Apellidos = request.Apellidos.Trim();
            usuario.Email = request.Email.Trim();
            usuario.DepartamentoId = departamentoObj?.Id;
            usuario.Departamento = departamentoObj; // 🔹 Permite que la auditoría automática extraiga el Nombre del departamento

            // 3. Guardar cambios usando Identity
            var result = await _userManager.UpdateAsync(usuario);
            if (!result.Succeeded)
            {
                var errorBase = result.Errors.FirstOrDefault();
                string mensajeTraducido = errorBase?.Code switch
                {
                    "DuplicateUserName" => "La identificación (Cédula) ingresada ya pertenece a otro funcionario.",
                    "DuplicateEmail" => $"El correo electrónico '{request.Email}' ya se encuentra registrado por otro funcionario.",
                    _ => errorBase?.Description ?? "No se pudieron actualizar los datos del funcionario."
                };

                return OperacionResultadoDto.Fallo(mensajeTraducido);
            }

            return OperacionResultadoDto.Exito($"Datos del funcionario '{usuario.Nombres} {usuario.Apellidos}' actualizados correctamente.");
        }
    }
}