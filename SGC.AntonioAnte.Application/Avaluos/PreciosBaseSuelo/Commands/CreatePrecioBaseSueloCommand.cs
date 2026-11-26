using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.PreciosBaseSuelo;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.PreciosBaseSuelo.Commands
{
    public record CreatePrecioBaseSueloCommand(CreatePrecioBaseSueloDto Dto) : IRequest<OperacionResultadoDto>;

    public class CreatePrecioBaseSueloValidator : AbstractValidator<CreatePrecioBaseSueloCommand>
    {
        public CreatePrecioBaseSueloValidator()
        {
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.TipoPredio).IsInEnum();
            RuleFor(v => v.Dto.Zona).NotEmpty().MaximumLength(10);
            RuleFor(v => v.Dto.Sector).NotEmpty().MaximumLength(10);
            RuleFor(v => v.Dto.ManzanaPoligono).MaximumLength(20);
            RuleFor(v => v.Dto.ValorMetroCuadrado).GreaterThan(0);
        }
    }

    public class CreatePrecioBaseSueloCommandHandler : IRequestHandler<CreatePrecioBaseSueloCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreatePrecioBaseSueloCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CreatePrecioBaseSueloCommand request, CancellationToken cancellationToken)
        {
            // Validar inmutabilidad de zonificación en el mismo año fiscal
            var existe = await _context.PreciosBaseSuelo
                .AnyAsync(p => p.AnioFiscal == request.Dto.AnioFiscal &&
                               p.TipoPredio == request.Dto.TipoPredio &&
                               p.Zona == request.Dto.Zona &&
                               p.Sector == request.Dto.Sector &&
                               p.ManzanaPoligono == request.Dto.ManzanaPoligono, cancellationToken);

            if (existe)
                return OperacionResultadoDto.Fallo("Ya existe un precio de suelo configurado para esta ubicación exacta en el año fiscal seleccionado.");

            var entidad = Domain.Avaluos.Entities.PrecioBaseSuelo.Crear(
                request.Dto.AnioFiscal,
                request.Dto.TipoPredio,
                request.Dto.Zona,
                request.Dto.Sector,
                request.Dto.ManzanaPoligono,
                request.Dto.ValorMetroCuadrado
            );

            _context.PreciosBaseSuelo.Add(entidad);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Precio base del suelo registrado exitosamente.", entidad.Id);
        }
    }
}