using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresDepreciacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.FactoresDepreciacion.Commands
{
    public record CreateFactorDepreciacionCommand(CreateFactorDepreciacionDto Dto) : IRequest<OperacionResultadoDto>;

    public class CreateFactorDepreciacionValidator : AbstractValidator<CreateFactorDepreciacionCommand>
    {
        public CreateFactorDepreciacionValidator()
        {
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.EstadoConservacionId).NotEmpty();
            RuleFor(v => v.Dto.EdadMaxima).GreaterThanOrEqualTo(v => v.Dto.EdadMinima).WithMessage("La edad máxima no puede ser menor a la edad mínima.");
            RuleFor(v => v.Dto.Coeficiente).InclusiveBetween(0.0001m, 1.0000m);
        }
    }

    public class CreateFactorDepreciacionCommandHandler : IRequestHandler<CreateFactorDepreciacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateFactorDepreciacionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CreateFactorDepreciacionCommand request, CancellationToken cancellationToken)
        {
            // Validar que el intervalo de edades no se solape con un rango ya existente para el mismo estado y año
            var solapamiento = await _context.FactoresDepreciacion
                .AnyAsync(f => f.AnioFiscal == request.Dto.AnioFiscal &&
                               f.EstadoConservacionId == request.Dto.EstadoConservacionId &&
                               f.EdadMinima <= request.Dto.EdadMaxima &&
                               f.EdadMaxima >= request.Dto.EdadMinima, cancellationToken);

            if (solapamiento)
                return OperacionResultadoDto.Fallo("El intervalo de edad se solapa con un rango de depreciación ya existente para este estado en el mismo año fiscal.");

            var entidad = Domain.Avaluos.Entities.FactorDepreciacion.Crear(
                request.Dto.AnioFiscal,
                request.Dto.EstadoConservacionId,
                request.Dto.EdadMinima,
                request.Dto.EdadMaxima,
                request.Dto.Coeficiente
            );

            _context.FactoresDepreciacion.Add(entidad);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Factor de depreciación registrado exitosamente.", entidad.Id);
        }
    }
}
