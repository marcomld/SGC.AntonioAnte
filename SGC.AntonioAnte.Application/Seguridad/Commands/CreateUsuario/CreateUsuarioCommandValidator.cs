using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.CreateUsuario
{
    public class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
    {
        public CreateUsuarioCommandValidator()
        {
            RuleFor(v => v.Identificacion)
                .NotEmpty().WithMessage("La identificación es obligatoria.")
                .Length(10).WithMessage("La cédula debe tener exactamente 10 dígitos.")
                .Matches("^[0-9]*$").WithMessage("La cédula solo debe contener números.");

            RuleFor(v => v.Nombres)
                .NotEmpty().WithMessage("Los nombres son obligatorios.")
                .MaximumLength(150).WithMessage("Los nombres no pueden exceder los 150 caracteres.");

            RuleFor(v => v.Apellidos)
                .NotEmpty().WithMessage("Los apellidos son obligatorios.")
                .MaximumLength(150).WithMessage("Los apellidos no pueden exceder los 150 caracteres.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");
            // Nota: Identity también hará sus validaciones de mayúsculas/números, pero paramos lo básico aquí.

            RuleFor(v => v.RolAsignado)
                .NotEmpty().WithMessage("Debe asignar un rol al funcionario para el sistema de catastros.");
        }
    }
}
