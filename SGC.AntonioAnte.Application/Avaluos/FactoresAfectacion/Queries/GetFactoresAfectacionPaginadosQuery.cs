using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresAfectacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.FactoresAfectacion.Queries
{
    public record GetFactoresAfectacionPaginadosQuery(
        int? AnioFiscal = null,
        string? TerminoBusqueda = null,
        bool? EstadoActivo = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<FactorAfectacionDto>>;

    public class GetFactoresAfectacionPaginadosQueryHandler : IRequestHandler<GetFactoresAfectacionPaginadosQuery, ResultadoPaginadoDto<FactorAfectacionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetFactoresAfectacionPaginadosQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ResultadoPaginadoDto<FactorAfectacionDto>> Handle(GetFactoresAfectacionPaginadosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.FactoresAfectacion.AsNoTracking().AsQueryable();

            if (request.AnioFiscal.HasValue)
                query = query.Where(f => f.AnioFiscal == request.AnioFiscal.Value);

            if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda))
                query = query.Where(f => f.TipoAfectacion.Contains(request.TerminoBusqueda));

            if (request.EstadoActivo.HasValue)
                query = query.Where(f => f.EstadoActivo == request.EstadoActivo.Value);

            var totalRegistros = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(f => f.AnioFiscal)
                .ThenBy(f => f.TipoAfectacion)
                .Skip((request.Pagina - 1) * request.RegistrosPorPagina)
                .Take(request.RegistrosPorPagina)
                .Select(f => new FactorAfectacionDto
                {
                    Id = f.Id,
                    AnioFiscal = f.AnioFiscal,
                    TipoAfectacion = f.TipoAfectacion,
                    Coeficiente = f.Coeficiente,
                    EstadoActivo = f.EstadoActivo
                })
                .ToListAsync(cancellationToken);

            return new ResultadoPaginadoDto<FactorAfectacionDto>
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = request.Pagina,
                RegistrosPorPagina = request.RegistrosPorPagina
            };
        }
    }
}