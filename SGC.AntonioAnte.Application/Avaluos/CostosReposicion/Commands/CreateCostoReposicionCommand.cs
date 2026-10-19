using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Application.Avaluos.CostosReposicion.Commands
{
    public record CreateCostoReposicionCommand(CreateCostoReposicionDto Dto) : IRequest<OperacionResultadoDto>;

    public class CreateCostoReposicionValidator : AbstractValidator<CreateCostoReposicionCommand>
    {
        public CreateCostoReposicionValidator()
        {
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000).WithMessage("El año fiscal es inválido.");
            RuleFor(v => v.Dto.TipoEstructuraId).NotEmpty().WithMessage("Debe seleccionar el tipo de estructura.");
            RuleFor(v => v.Dto.ValorMetroCuadrado).GreaterThan(0).WithMessage("El valor por m² debe ser positivo.");
        }
    }

    public class CreateCostoReposicionCommandHandler : IRequestHandler<CreateCostoReposicionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateCostoReposicionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CreateCostoReposicionCommand request, CancellationToken cancellationToken)
        {
            var existe = await _context.CostosReposicion
                .AnyAsync(c => c.AnioFiscal == request.Dto.AnioFiscal && c.TipoEstructuraId == request.Dto.TipoEstructuraId, cancellationToken);

            if (existe)
                return OperacionResultadoDto.Fallo($"Ya existe un costo de reposición para esta estructura en el año {request.Dto.AnioFiscal}.");

            var entidad = Domain.Avaluos.Entities.CostoReposicion.Crear(
                request.Dto.AnioFiscal,
                request.Dto.TipoEstructuraId,
                request.Dto.ValorMetroCuadrado
            );

            _context.CostosReposicion.Add(entidad);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Costo de reposición parametrizado exitosamente.", entidad.Id);
        }
    }
}