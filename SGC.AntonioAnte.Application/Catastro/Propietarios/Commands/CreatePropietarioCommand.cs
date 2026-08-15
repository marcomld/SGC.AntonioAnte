using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Propietarios.Commands
{
    public record CreatePropietarioCommand(
        TipoPropietario TipoPropietario,
        string Identificacion,
        string? Nombres,
        string? Apellidos,
        string? RazonSocial,
        EstadoCivil EstadoCivil,
        string? Email,
        string? Telefono
    ) : IRequest<Guid>;

    public class CreatePropietarioCommandValidator : AbstractValidator<CreatePropietarioCommand>
    {
        public CreatePropietarioCommandValidator()
        {
            RuleFor(v => v.Identificacion)
                .NotEmpty().WithMessage("La identificación (Cédula o RUC) es obligatoria.")
                .Must(ValidarIdentificacionEcuatoriana).WithMessage("La identificación ingresada no cumple con el formato válido para Ecuador.");

            When(v => v.TipoPropietario == TipoPropietario.Natural, () =>
            {
                RuleFor(v => v.Nombres)
                    .NotEmpty().WithMessage("Los nombres son obligatorios para personas naturales.")
                    .MaximumLength(100).WithMessage("Los nombres no pueden exceder los 100 caracteres.");

                RuleFor(v => v.Apellidos)
                    .NotEmpty().WithMessage("Los apellidos son obligatorios para personas naturales.")
                    .MaximumLength(100).WithMessage("Los apellidos no pueden exceder los 100 caracteres.");
            });

            When(v => v.TipoPropietario == TipoPropietario.Juridico, () =>
            {
                RuleFor(v => v.RazonSocial)
                    .NotEmpty().WithMessage("La razón social es obligatoria para personas jurídicas.")
                    .MaximumLength(200).WithMessage("La razón social no puede exceder los 200 caracteres.");
            });
        }

        private bool ValidarIdentificacionEcuatoriana(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion)) return false;
            var id = identificacion.Trim();

            // Cédula de 10 dígitos
            if (id.Length == 10) return ValidarCedula(id);

            // RUC de 13 dígitos
            if (id.Length == 13) return id.EndsWith("001") && (ValidarCedula(id.Substring(0, 10)) || id.StartsWith("179") || id.StartsWith("109"));

            return false;
        }

        private bool ValidarCedula(string cedula)
        {
            if (cedula.Length != 10 || !long.TryParse(cedula, out _)) return false;

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24) return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito < 0 || tercerDigito > 5) return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula.Substring(i, 1)) * coeficientes[i];
                if (valor > 9) valor -= 9;
                suma += valor;
            }

            int digitoVerificador = int.Parse(cedula.Substring(9, 1));
            int decenaSuperior = ((suma + 9) / 10) * 10;
            int resultado = decenaSuperior - suma;
            if (resultado == 10) resultado = 0;

            return resultado == digitoVerificador;
        }
    }

    public class CreatePropietarioCommandHandler : IRequestHandler<CreatePropietarioCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreatePropietarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreatePropietarioCommand request, CancellationToken cancellationToken)
        {
            // Validar unicidad de Identificación
            var existe = await _context.Propietarios
                .AnyAsync(p => p.Identificacion == request.Identificacion && p.EstadoActivo, cancellationToken);

            if (existe)
            {
                throw new Exception($"El ciudadano/empresa con identificación '{request.Identificacion}' ya se encuentra registrado en el sistema.");
            }

            Propietario nuevo;

            if (request.TipoPropietario == TipoPropietario.Natural)
            {
                nuevo = Propietario.CrearPersonaNatural(
                    request.Identificacion,
                    request.Nombres!,
                    request.Apellidos!,
                    request.EstadoCivil,
                    request.Email,
                    request.Telefono);
            }
            else
            {
                nuevo = Propietario.CrearPersonaJuridica(
                    request.Identificacion,
                    request.RazonSocial!,
                    request.Email,
                    request.Telefono);
            }

            _context.Propietarios.Add(nuevo);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevo.Id;
        }
    }
}
