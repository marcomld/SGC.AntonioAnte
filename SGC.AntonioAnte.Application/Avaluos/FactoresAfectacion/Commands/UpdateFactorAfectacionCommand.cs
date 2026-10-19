using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresAfectacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.FactoresAfectacion.Commands
{
    public record UpdateFactorAfectacionCommand(UpdateFactorAfectacionDto Dto) : IRequest<OperacionResultadoDto>;

    public class UpdateFactorAfectacionValidator : AbstractValidator<UpdateFactorAfectacionCommand>
    {
        public UpdateFactorAfectacionValidator()
        {
            RuleFor(v => v.Dto.Id).NotEmpty();
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.TipoAfectacion).NotEmpty().MaximumLength(100);
            RuleFor(v => v.Dto.Coeficiente).InclusiveBetween(0.0001m, 2.0000m);
        }
    }

    public class UpdateFactorAfectacionCommandHandler : IRequestHandler<UpdateFactorAfectacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdateFactorAfectacionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(UpdateFactorAfectacionCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.FactoresAfectacion.FindAsync(new object[] { request.Dto.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("El factor de afectación no existe.");

            var duplicado = await _context.FactoresAfectacion
                .AnyAsync(f => f.Id != request.Dto.Id && f.AnioFiscal == request.Dto.AnioFiscal && f.TipoAfectacion == request.Dto.TipoAfectacion, cancellationToken);

            if (duplicado)
                return OperacionResultadoDto.Fallo("Ya existe otro factor de afectación con el mismo nombre para este año fiscal.");

            entidad.Actualizar(request.Dto.AnioFiscal, request.Dto.TipoAfectacion, request.Dto.Coeficiente);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Factor de afectación actualizado correctamente.");
        }
    }
}