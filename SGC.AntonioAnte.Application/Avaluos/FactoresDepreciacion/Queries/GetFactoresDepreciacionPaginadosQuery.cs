using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresDepreciacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.FactoresDepreciacion.Queries
{
    public record GetFactoresDepreciacionPaginadosQuery(
        int? AnioFiscal = null,
        Guid? EstadoConservacionId = null,
        bool? EstadoActivo = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<FactorDepreciacionDto>>;

    public class GetFactoresDepreciacionPaginadosQueryHandler : IRequestHandler<GetFactoresDepreciacionPaginadosQuery, ResultadoPaginadoDto<FactorDepreciacionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetFactoresDepreciacionPaginadosQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ResultadoPaginadoDto<FactorDepreciacionDto>> Handle(GetFactoresDepreciacionPaginadosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.FactoresDepreciacion
                .Include(f => f.EstadoConservacion)
                .AsNoTracking()
                .AsQueryable();

            if (request.AnioFiscal.HasValue)
                query = query.Where(f => f.AnioFiscal == request.AnioFiscal.Value);

            if (request.EstadoConservacionId.HasValue && request.EstadoConservacionId.Value != Guid.Empty)
                query = query.Where(f => f.EstadoConservacionId == request.EstadoConservacionId.Value);

            if (request.EstadoActivo.HasValue)
                query = query.Where(f => f.EstadoActivo == request.EstadoActivo.Value);

            var totalRegistros = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(f => f.AnioFiscal)
                .ThenBy(f => f.EstadoConservacion!.Nombre)
                .ThenBy(f => f.EdadMinima)
                .Skip((request.Pagina - 1) * request.RegistrosPorPagina)
                .Take(request.RegistrosPorPagina)
                .Select(f => new FactorDepreciacionDto
                {
                    Id = f.Id,
                    AnioFiscal = f.AnioFiscal,
                    EstadoConservacionId = f.EstadoConservacionId,
                    EstadoConservacionNombre = f.EstadoConservacion!.Nombre,
                    EdadMinima = f.EdadMinima,
                    EdadMaxima = f.EdadMaxima,
                    Coeficiente = f.Coeficiente,
                    EstadoActivo = f.EstadoActivo
                })
                .ToListAsync(cancellationToken);

            return new ResultadoPaginadoDto<FactorDepreciacionDto>
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = request.Pagina,
                RegistrosPorPagina = request.RegistrosPorPagina
            };
        }
    }
}
