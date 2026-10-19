using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Application.Avaluos.CostosReposicion.Commands
{
    public record UpdateCostoReposicionCommand(UpdateCostoReposicionDto Dto) : IRequest<OperacionResultadoDto>;

    public class UpdateCostoReposicionValidator : AbstractValidator<UpdateCostoReposicionCommand>
    {
        public UpdateCostoReposicionValidator()
        {
            RuleFor(v => v.Dto.Id).NotEmpty();
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.ValorMetroCuadrado).GreaterThan(0);
        }
    }

    public class UpdateCostoReposicionCommandHandler : IRequestHandler<UpdateCostoReposicionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCostoReposicionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(UpdateCostoReposicionCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.CostosReposicion.FindAsync(new object[] { request.Dto.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("El costo de reposición no existe.");

            var duplicado = await _context.CostosReposicion
                .AnyAsync(c => c.AnioFiscal == request.Dto.AnioFiscal &&
                               c.TipoEstructuraId == request.Dto.TipoEstructuraId &&
                               c.Id != request.Dto.Id, cancellationToken);

            if (duplicado)
                return OperacionResultadoDto.Fallo("Ya existe otro registro para este tipo de estructura en el mismo año fiscal.");

            entidad.Actualizar(request.Dto.AnioFiscal, request.Dto.TipoEstructuraId, request.Dto.ValorMetroCuadrado);
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Costo de reposición actualizado correctamente.");
        }
    }
}