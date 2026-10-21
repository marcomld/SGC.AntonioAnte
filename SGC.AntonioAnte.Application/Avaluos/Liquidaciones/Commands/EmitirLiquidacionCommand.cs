using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Avaluos.Simulacion.Queries;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.Liquidaciones.Commands
{
    public record EmitirLiquidacionCommand(EmitirLiquidacionDto Dto) : IRequest<OperacionResultadoDto>;

    public class EmitirLiquidacionValidator : AbstractValidator<EmitirLiquidacionCommand>
    {
        public EmitirLiquidacionValidator()
        {
            RuleFor(v => v.Dto.PredioId).NotEmpty().WithMessage("El ID del predio es requerido.");
            RuleFor(v => v.Dto.AnioFiscal).GreaterThan(2000).WithMessage("El año fiscal no es válido.");
        }
    }

    public class EmitirLiquidacionCommandHandler : IRequestHandler<EmitirLiquidacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _mediator; // Inyectamos MediatR para reutilizar el simulador

        public EmitirLiquidacionCommandHandler(IApplicationDbContext context, ISender mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<OperacionResultadoDto> Handle(EmitirLiquidacionCommand request, CancellationToken cancellationToken)
        {
            // 1. REGLA DE NEGOCIO: Evitar doble emisión (doble tributación)
            var yaEmitido = await _context.LiquidacionesPrediales
                .AnyAsync(l => l.PredioId == request.Dto.PredioId &&
                               l.AnioFiscal == request.Dto.AnioFiscal &&
                               l.EsSimulacion == false, cancellationToken);

            if (yaEmitido)
                return OperacionResultadoDto.Fallo($"Ya existe una liquidación emitida en firme para este predio en el año fiscal {request.Dto.AnioFiscal}.");

            // 2. REUTILIZAR EL MOTOR MATEMÁTICO (Simulador)
            var querySimulacion = new SimularAvaluoQuery(request.Dto.PredioId, request.Dto.AnioFiscal);
            var resultadoSimulacion = await _mediator.Send(querySimulacion, cancellationToken);

            if (!resultadoSimulacion.Exitoso || resultadoSimulacion.Datos == null)
                return OperacionResultadoDto.Fallo($"No se pudo emitir la liquidación debido a un error en el motor de cálculo: {resultadoSimulacion.Mensaje}");

            var simulacion = (AvaluoSimuladoDto)resultadoSimulacion.Datos;

            // 3. INMUTAR EL RESULTADO (Crear el Hecho Generador en Base de Datos)
            // Nota: ExoneracionesPrevias y CemAplicada las enviamos en 0 para este MVP.
            var liquidacion = Domain.Avaluos.Entities.LiquidacionPredial.Crear(
                predioId: simulacion.PredioId,
                anioFiscal: simulacion.AnioFiscal,
                esSimulacion: false, // 🔹 Esto es clave: Ya no es simulación, es oficial
                valorTerreno: simulacion.AvaluoSueloFinal,
                valorConstruccion: simulacion.AvaluoEdificacionesFinal,
                exoneracionesPrevias: 0m,
                cemAplicada: 0m,
                estado: "Emitido"
            );

            _context.LiquidacionesPrediales.Add(liquidacion);
            await _context.SaveChangesAsync(cancellationToken);

            // Retornamos el ID de la liquidación oficial
            return OperacionResultadoDto.Exito("Liquidación emitida y guardada en firme exitosamente.", liquidacion.Id);
        }
    }
}
