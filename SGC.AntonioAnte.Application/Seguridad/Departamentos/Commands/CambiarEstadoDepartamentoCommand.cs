using FluentValidation;
using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands
{
    // 1. COMMAND
    public record CambiarEstadoDepartamentoCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class CambiarEstadoDepartamentoCommandValidator : AbstractValidator<CambiarEstadoDepartamentoCommand>
    {
        public CambiarEstadoDepartamentoCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El identificador del departamento es obligatorio.");
        }
    }

    // 3. HANDLER
    public class CambiarEstadoDepartamentoCommandHandler : IRequestHandler<CambiarEstadoDepartamentoCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoDepartamentoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null)
            {
                return OperacionResultadoDto.Fallo("Departamento no encontrado.");
            }

            // Invertimos el estado actual
            dep.EstadoActivo = !dep.EstadoActivo;

            // 🔹 El DbContext intercepta el cambio en 'EstadoActivo' y audita ACTUALIZAR_DEPARTAMENTO automáticamente
            await _context.SaveChangesAsync(cancellationToken);

            string mensajeEstado = dep.EstadoActivo ? "activado" : "desactivado";
            return OperacionResultadoDto.Exito($"El departamento '{dep.Nombre}' fue {mensajeEstado} exitosamente.");
        }
    }
}