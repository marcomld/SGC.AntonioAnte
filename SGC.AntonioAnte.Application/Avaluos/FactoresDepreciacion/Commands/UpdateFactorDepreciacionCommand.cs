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
    public record UpdateFactorDepreciacionCommand(UpdateFactorDepreciacionDto Dto) : IRequest<OperacionResultadoDto>;

    public class UpdateFactorDepreciacionValidator : AbstractValidator<UpdateFactorDepreciacionCommand>
    {
        public UpdateFactorDepreciacionValidator()
        {
            RuleFor(v => v.Dto.Id).NotEmpty();
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.EdadMaxima).GreaterThanOrEqualTo(v => v.Dto.EdadMinima).WithMessage("La edad máxima no puede ser menor a la mínima.");
            RuleFor(v => v.Dto.Coeficiente).InclusiveBetween(0.0001m, 1.0000m);
        }
    }

    public class UpdateFactorDepreciacionCommandHandler : IRequestHandler<UpdateFactorDepreciacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdateFactorDepreciacionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(UpdateFactorDepreciacionCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.FactoresDepreciacion.FindAsync(new object[] { request.Dto.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("El factor de depreciación no existe.");

            var solapamiento = await _context.FactoresDepreciacion
                .AnyAsync(f => f.Id != request.Dto.Id &&
                               f.AnioFiscal == request.Dto.AnioFiscal &&
                               f.EstadoConservacionId == request.Dto.EstadoConservacionId &&
                               f.EdadMinima <= request.Dto.EdadMaxima &&
                               f.EdadMaxima >= request.Dto.EdadMinima, cancellationToken);

            if (solapamiento)
                return OperacionResultadoDto.Fallo("El intervalo de edad modificado se solapa con un rango ya existente.");

            entidad.Actualizar(request.Dto.AnioFiscal, request.Dto.EstadoConservacionId, request.Dto.EdadMinima, request.Dto.EdadMaxima, request.Dto.Coeficiente);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Factor de depreciación actualizado correctamente.");
        }
    }
}
