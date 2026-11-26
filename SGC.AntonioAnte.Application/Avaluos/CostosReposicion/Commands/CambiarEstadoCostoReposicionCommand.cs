using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Application.Avaluos.CostosReposicion.Commands
{
    public record CambiarEstadoCostoReposicionCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    public class CambiarEstadoCostoReposicionCommandHandler : IRequestHandler<CambiarEstadoCostoReposicionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoCostoReposicionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoCostoReposicionCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.CostosReposicion.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("Registro no encontrado.");

            entidad.EstadoActivo = !entidad.EstadoActivo;
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"El costo de reposición fue {(entidad.EstadoActivo ? "activado" : "desactivado")} exitosamente.");
        }
    }
}