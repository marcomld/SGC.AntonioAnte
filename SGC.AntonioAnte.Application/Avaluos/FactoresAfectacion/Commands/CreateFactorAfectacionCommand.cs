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
    public record CreateFactorAfectacionCommand(CreateFactorAfectacionDto Dto) : IRequest<OperacionResultadoDto>;

    public class CreateFactorAfectacionValidator : AbstractValidator<CreateFactorAfectacionCommand>
    {
        public CreateFactorAfectacionValidator()
        {
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.TipoAfectacion).NotEmpty().MaximumLength(100);
            RuleFor(v => v.Dto.Coeficiente).InclusiveBetween(0.0001m, 2.0000m);
        }
    }

    public class CreateFactorAfectacionCommandHandler : IRequestHandler<CreateFactorAfectacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateFactorAfectacionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CreateFactorAfectacionCommand request, CancellationToken cancellationToken)
        {
            var duplicado = await _context.FactoresAfectacion
                .AnyAsync(f => f.AnioFiscal == request.Dto.AnioFiscal && f.TipoAfectacion == request.Dto.TipoAfectacion, cancellationToken);

            if (duplicado)
                return OperacionResultadoDto.Fallo("Ya existe un factor de afectación con ese nombre para el año fiscal seleccionado.");

            var entidad = Domain.Avaluos.Entities.FactorAfectacion.Crear(
                request.Dto.AnioFiscal,
                request.Dto.TipoAfectacion,
                request.Dto.Coeficiente
            );

            _context.FactoresAfectacion.Add(entidad);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Factor de afectación registrado exitosamente.", entidad.Id);
        }
    }
}