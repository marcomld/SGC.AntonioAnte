using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
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
    ) : IRequest<OperacionResultadoDto>;

    public class CreatePropietarioCommandValidator : AbstractValidator<CreatePropietarioCommand>
    {
        public CreatePropietarioCommandValidator()
        {
            RuleFor(v => v.Identificacion)
                .NotEmpty().WithMessage("La identificación (Cédula o RUC) es obligatoria.")
                .Must(ValidarDocumentoEcuatoriano)
                .WithMessage("El número de documento no es una Cédula o RUC válido para el territorio ecuatoriano.");

            When(v => v.TipoPropietario == TipoPropietario.Natural, () =>
            {
                RuleFor(v => v.Nombres)
                    .NotEmpty().WithMessage("Los nombres son obligatorios para personas naturales.")
                    .MaximumLength(100).WithMessage("Los nombres no pueden exceder los 100 caracteres.");

                RuleFor(v => v.Apellidos)
                    .NotEmpty().WithMessage("Los apellidos son obligatorios para personas naturales.")
                    .MaximumLength(100).WithMessage("Los apellidos no pueden exceder los 100 caracteres.");

                RuleFor(v => v.Identificacion)
                    .Must(id => id.Length == 10 || (id.Length == 13 && id.EndsWith("001")))
                    .WithMessage("La persona natural debe poseer Cédula (10 dígitos) o RUC Natural (13 dígitos).");
            });

            When(v => v.TipoPropietario == TipoPropietario.Juridico, () =>
            {
                RuleFor(v => v.RazonSocial)
                    .NotEmpty().WithMessage("La razón social es obligatoria para personas jurídicas.")
                    .MaximumLength(200).WithMessage("La razón social no puede exceder los 200 caracteres.");

                RuleFor(v => v.Identificacion)
                    .Length(13).WithMessage("La persona jurídica debe registrar obligatoriamente un RUC de 13 dígitos.");
            });
        }

        private bool ValidarDocumentoEcuatoriano(string? documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return false;
            var doc = documento.Trim();

            if (doc.Length != 10 && doc.Length != 13) return false;
            if (!long.TryParse(doc, out _)) return false;

            int provincia = int.Parse(doc.Substring(0, 2));
            if ((provincia < 1 || provincia > 24) && provincia != 30) return false;

            int tercerDigito = int.Parse(doc.Substring(2, 1));

            // A. PERSONA NATURAL (Cédula o RUC Natural - Módulo 10)
            if (tercerDigito < 6)
            {
                if (doc.Length == 13 && !doc.EndsWith("001")) return false;

                int[] coef = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
                int suma = 0;
                for (int i = 0; i < 9; i++)
                {
                    int val = int.Parse(doc[i].ToString()) * coef[i];
                    suma += (val >= 10) ? val - 9 : val;
                }
                int verificadorCalculado = (suma % 10 == 0) ? 0 : 10 - (suma % 10);
                int verificadorReal = int.Parse(doc[9].ToString());

                return verificadorCalculado == verificadorReal;
            }
            // B. SOCIEDAD PRIVADA / EXTRANJERA (RUC Jurídico - Módulo 11)
            else if (tercerDigito == 9)
            {
                if (doc.Length != 13 || !doc.EndsWith("001")) return false;

                int[] coef = { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                int suma = 0;
                for (int i = 0; i < 9; i++)
                {
                    suma += int.Parse(doc[i].ToString()) * coef[i];
                }
                int residuo = suma % 11;
                int verificadorCalculado = (residuo == 0) ? 0 : 11 - residuo;
                int verificadorReal = int.Parse(doc[9].ToString());

                return verificadorCalculado == verificadorReal;
            }
            // C. INSTITUCIÓN PÚBLICA (RUC Público - Módulo 11)
            else if (tercerDigito == 6)
            {
                if (doc.Length != 13 || !doc.EndsWith("0001")) return false;

                int[] coef = { 3, 2, 7, 6, 5, 4, 3, 2 };
                int suma = 0;
                for (int i = 0; i < 8; i++)
                {
                    suma += int.Parse(doc[i].ToString()) * coef[i];
                }
                int residuo = suma % 11;
                int verificadorCalculado = (residuo == 0) ? 0 : 11 - residuo;
                int verificadorReal = int.Parse(doc[8].ToString());

                return verificadorCalculado == verificadorReal;
            }

            return false;
        }
    }

    public class CreatePropietarioCommandHandler : IRequestHandler<CreatePropietarioCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreatePropietarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CreatePropietarioCommand request, CancellationToken cancellationToken)
        {
            var existe = await _context.Propietarios
                .AnyAsync(p => p.Identificacion == request.Identificacion.Trim() && p.EstadoActivo, cancellationToken);

            if (existe)
            {
                return OperacionResultadoDto.Fallo($"El ciudadano/empresa con identificación '{request.Identificacion}' ya se encuentra registrado en el sistema.");
            }

            Propietario nuevo;

            if (request.TipoPropietario == TipoPropietario.Natural)
            {
                nuevo = Propietario.CrearPersonaNatural(
                    request.Identificacion.Trim(),
                    request.Nombres!.Trim(),
                    request.Apellidos!.Trim(),
                    request.EstadoCivil,
                    request.Email?.Trim(),
                    request.Telefono?.Trim());
            }
            else
            {
                nuevo = Propietario.CrearPersonaJuridica(
                    request.Identificacion.Trim(),
                    request.RazonSocial!.Trim(),
                    request.Email?.Trim(),
                    request.Telefono?.Trim());
            }

            _context.Propietarios.Add(nuevo);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Propietario registrado exitosamente.");
        }
    }
}