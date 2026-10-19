using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.CostosReposicion.Queries
{
    public record GetCostosReposicionPaginadosQuery(
        int? AnioFiscal = null,
        Guid? TipoEstructuraId = null,
        bool? EstadoActivo = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<CostoReposicionDto>>;

    public class GetCostosReposicionPaginadosQueryHandler : IRequestHandler<GetCostosReposicionPaginadosQuery, ResultadoPaginadoDto<CostoReposicionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCostosReposicionPaginadosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<CostoReposicionDto>> Handle(GetCostosReposicionPaginadosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CostosReposicion
                .Include(c => c.TipoEstructura)
                .AsNoTracking()
                .AsQueryable();

            if (request.AnioFiscal.HasValue)
                query = query.Where(c => c.AnioFiscal == request.AnioFiscal.Value);

            if (request.TipoEstructuraId.HasValue && request.TipoEstructuraId.Value != Guid.Empty)
                query = query.Where(c => c.TipoEstructuraId == request.TipoEstructuraId.Value);

            if (request.EstadoActivo.HasValue)
                query = query.Where(c => c.EstadoActivo == request.EstadoActivo.Value);

            var totalRegistros = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.AnioFiscal)
                .ThenBy(c => c.TipoEstructura!.Nombre)
                .Skip((request.Pagina - 1) * request.RegistrosPorPagina)
                .Take(request.RegistrosPorPagina)
                .Select(c => new CostoReposicionDto
                {
                    Id = c.Id,
                    AnioFiscal = c.AnioFiscal,
                    TipoEstructuraId = c.TipoEstructuraId,
                    TipoEstructuraNombre = c.TipoEstructura!.Nombre,
                    ValorMetroCuadrado = c.ValorMetroCuadrado,
                    EstadoActivo = c.EstadoActivo
                })
                .ToListAsync(cancellationToken);

            return new ResultadoPaginadoDto<CostoReposicionDto>
            {
                Items = items, // 🔹 Cambiado de Registros a Items
                TotalRegistros = totalRegistros,
                PaginaActual = request.Pagina,
                RegistrosPorPagina = request.RegistrosPorPagina
            };
        }
    }
}
