using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.Claims
{
    public class AddClaimCommandValidator : AbstractValidator<AddClaimCommand>
    {
        public AddClaimCommandValidator()
        {
            RuleFor(v => v.UsuarioId)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio.");

            RuleFor(v => v.ClaimType)
                .NotEmpty().WithMessage("El tipo de permiso (ClaimType) es obligatorio.");

            RuleFor(v => v.ClaimValue)
                .NotEmpty().WithMessage("El valor del permiso (ClaimValue) es obligatorio.");
        }
    }
}
