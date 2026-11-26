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
    public record UpdatePrecioBaseSueloCommand(UpdatePrecioBaseSueloDto Dto) : IRequest<OperacionResultadoDto>;

    public class UpdatePrecioBaseSueloValidator : AbstractValidator<UpdatePrecioBaseSueloCommand>
    {
        public UpdatePrecioBaseSueloValidator()
        {
            RuleFor(v => v.Dto.Id).NotEmpty();
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.TipoPredio).IsInEnum();
            RuleFor(v => v.Dto.Zona).NotEmpty().MaximumLength(10);
            RuleFor(v => v.Dto.Sector).NotEmpty().MaximumLength(10);
            RuleFor(v => v.Dto.ManzanaPoligono).MaximumLength(20);
            RuleFor(v => v.Dto.ValorMetroCuadrado).GreaterThan(0);
        }
    }

    public class UpdatePrecioBaseSueloCommandHandler : IRequestHandler<UpdatePrecioBaseSueloCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdatePrecioBaseSueloCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(UpdatePrecioBaseSueloCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.PreciosBaseSuelo.FindAsync(new object[] { request.Dto.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("El registro de precio no existe.");

            var duplicado = await _context.PreciosBaseSuelo
                .AnyAsync(p => p.Id != request.Dto.Id &&
                               p.AnioFiscal == request.Dto.AnioFiscal &&
                               p.TipoPredio == request.Dto.TipoPredio &&
                               p.Zona == request.Dto.Zona &&
                               p.Sector == request.Dto.Sector &&
                               p.ManzanaPoligono == request.Dto.ManzanaPoligono, cancellationToken);

            if (duplicado)
                return OperacionResultadoDto.Fallo("Ya existe otro registro configurado para esta ubicación exacta en el año fiscal seleccionado.");

            entidad.Actualizar(request.Dto.AnioFiscal, request.Dto.TipoPredio, request.Dto.Zona, request.Dto.Sector, request.Dto.ManzanaPoligono, request.Dto.ValorMetroCuadrado);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Precio base del suelo actualizado correctamente.");
        }
    }
}