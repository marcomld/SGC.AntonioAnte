using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.PreciosBaseSuelo;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.PreciosBaseSuelo.Queries
{
    public record GetPreciosBaseSueloPaginadosQuery(
        int? AnioFiscal = null,
        TipoPredio? TipoPredio = null,
        string? TerminoBusqueda = null,
        bool? EstadoActivo = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<PrecioBaseSueloDto>>;

    public class GetPreciosBaseSueloPaginadosQueryHandler : IRequestHandler<GetPreciosBaseSueloPaginadosQuery, ResultadoPaginadoDto<PrecioBaseSueloDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPreciosBaseSueloPaginadosQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ResultadoPaginadoDto<PrecioBaseSueloDto>> Handle(GetPreciosBaseSueloPaginadosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PreciosBaseSuelo.AsNoTracking().AsQueryable();

            if (request.AnioFiscal.HasValue)
                query = query.Where(p => p.AnioFiscal == request.AnioFiscal.Value);

            if (request.TipoPredio.HasValue)
                query = query.Where(p => p.TipoPredio == request.TipoPredio.Value);

            if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda))
                query = query.Where(p => p.Zona.Contains(request.TerminoBusqueda) ||
                                         p.Sector.Contains(request.TerminoBusqueda) ||
                                         p.ManzanaPoligono.Contains(request.TerminoBusqueda));

            if (request.EstadoActivo.HasValue)
                query = query.Where(p => p.EstadoActivo == request.EstadoActivo.Value);

            var totalRegistros = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.AnioFiscal)
                .ThenBy(p => p.TipoPredio)
                .ThenBy(p => p.Zona)
                .ThenBy(p => p.Sector)
                .Skip((request.Pagina - 1) * request.RegistrosPorPagina)
                .Take(request.RegistrosPorPagina)
                .Select(p => new PrecioBaseSueloDto
                {
                    Id = p.Id,
                    AnioFiscal = p.AnioFiscal,
                    TipoPredio = p.TipoPredio,
                    Zona = p.Zona,
                    Sector = p.Sector,
                    ManzanaPoligono = p.ManzanaPoligono,
                    ValorMetroCuadrado = p.ValorMetroCuadrado,
                    EstadoActivo = p.EstadoActivo
                })
                .ToListAsync(cancellationToken);

            return new ResultadoPaginadoDto<PrecioBaseSueloDto>
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = request.Pagina,
                RegistrosPorPagina = request.RegistrosPorPagina
            };
        }
    }
}