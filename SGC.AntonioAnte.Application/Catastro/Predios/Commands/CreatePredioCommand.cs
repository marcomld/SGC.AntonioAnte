using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Domain.Catastro.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Commands
{
    public record CreatePredioCommand(
        string ClaveCatastral,
        string? ClaveAnterior,
        TipoPredio TipoPredio,
        decimal AreaTerrenoEscritura,
        decimal AreaTerrenoGrafica,
        string Direccion,
        string? PoligonoWkt
    ) : IRequest<Guid>;

    public class CreatePredioCommandValidator : AbstractValidator<CreatePredioCommand>
    {
        public CreatePredioCommandValidator()
        {
            RuleFor(v => v.ClaveCatastral)
                .NotEmpty().WithMessage("La clave catastral es obligatoria.");

            RuleFor(v => v.AreaTerrenoEscritura)
                .GreaterThan(0).WithMessage("El área según escritura debe ser mayor a 0.");

            RuleFor(v => v.Direccion)
                .NotEmpty().WithMessage("La dirección o referencia física del predio es obligatoria.")
                .MaximumLength(250).WithMessage("La dirección no puede exceder los 250 caracteres.");
        }
    }

    public class CreatePredioCommandHandler : IRequestHandler<CreatePredioCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreatePredioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreatePredioCommand request, CancellationToken cancellationToken)
        {
            // Instanciación y validación DPA del Value Object
            var claveVO = ClaveCatastral.Crear(request.ClaveCatastral);

            // CORRECCIÓN: Comparamos directamente contra el Value Object
            var existe = await _context.Predios
                .AnyAsync(p => p.ClaveCatastral == claveVO && p.EstadoActivo, cancellationToken);

            if (existe)
            {
                throw new Exception($"Ya existe un predio activo registrado con la clave catastral '{claveVO.Valor}'.");
            }

            // Conversión de formato WKT a Geometría espacial de NetTopologySuite
            Geometry? poligonoGeometry = null;
            if (!string.IsNullOrWhiteSpace(request.PoligonoWkt))
            {
                try
                {
                    var reader = new WKTReader();
                    poligonoGeometry = reader.Read(request.PoligonoWkt);
                    poligonoGeometry.SRID = 4326; // WGS84
                }
                catch (Exception ex)
                {
                    throw new Exception($"El formato espacial WKT ingresado no es válido: {ex.Message}");
                }
            }

            var nuevoPredio = Predio.Crear(
                claveVO,
                request.ClaveAnterior,
                request.TipoPredio,
                request.AreaTerrenoEscritura,
                request.AreaTerrenoGrafica,
                request.Direccion,
                poligonoGeometry
            );

            _context.Predios.Add(nuevoPredio);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoPredio.Id;
        }
    }
}
