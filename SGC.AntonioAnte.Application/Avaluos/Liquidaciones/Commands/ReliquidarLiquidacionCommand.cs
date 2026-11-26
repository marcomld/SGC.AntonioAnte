using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Avaluos.Simulacion.Queries;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.Liquidaciones.Commands
{
    public record ReliquidarLiquidacionCommand(ReliquidarLiquidacionDto Dto) : IRequest<OperacionResultadoDto>;

    public class ReliquidarLiquidacionValidator : AbstractValidator<ReliquidarLiquidacionCommand>
    {
        public ReliquidarLiquidacionValidator()
        {
            RuleFor(v => v.Dto.PredioId).NotEmpty();
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000);
            RuleFor(v => v.Dto.MotivoReliquidacion).NotEmpty().MaximumLength(500);
        }
    }

    public class ReliquidarLiquidacionCommandHandler : IRequestHandler<ReliquidarLiquidacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _mediator;

        public ReliquidarLiquidacionCommandHandler(IApplicationDbContext context, ISender mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<OperacionResultadoDto> Handle(ReliquidarLiquidacionCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar la liquidación actual (Emitida)
            var liquidacionActual = await _context.LiquidacionesPrediales
                .FirstOrDefaultAsync(l => l.PredioId == request.Dto.PredioId &&
                                          l.AnioFiscal == request.Dto.AnioFiscal &&
                                          l.EsSimulacion == false &&
                                          l.EstadoLiquidacion == "Emitido", cancellationToken);

            if (liquidacionActual == null)
                return OperacionResultadoDto.Fallo("No existe una liquidación vigente para reliquidar en este año fiscal.");

            // 2. Dar de baja la actual conservando auditoría
            liquidacionActual.AnularOSustituir(request.Dto.MotivoReliquidacion);

            // 3. Ejecutar el Motor Matemático actualizado
            var resultadoSimulacion = await _mediator.Send(new SimularAvaluoQuery(request.Dto.PredioId, request.Dto.AnioFiscal), cancellationToken);

            if (!resultadoSimulacion.Exitoso || resultadoSimulacion.Datos == null)
                return OperacionResultadoDto.Fallo($"Error al recalcular el nuevo avalúo: {resultadoSimulacion.Mensaje}");

            var simulacion = (AvaluoSimuladoDto)resultadoSimulacion.Datos;

            // 4. Crear la Nueva Liquidación Vigente
            var nuevaLiquidacion = Domain.Avaluos.Entities.LiquidacionPredial.Crear(
                predioId: simulacion.PredioId,
                anioFiscal: simulacion.AnioFiscal,
                esSimulacion: false,
                valorTerreno: simulacion.AvaluoSueloFinal,
                valorConstruccion: simulacion.AvaluoEdificacionesFinal,
                exoneracionesPrevias: 0m,
                cemAplicada: 0m,
                estado: "Emitido" // Renace como Emitido
            );

            _context.LiquidacionesPrediales.Add(nuevaLiquidacion);

            // EF Core hará el Update del viejo y el Insert del nuevo en la misma transacción
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Liquidación sustituida y reliquidada con éxito.", nuevaLiquidacion.Id);
        }
    }
}