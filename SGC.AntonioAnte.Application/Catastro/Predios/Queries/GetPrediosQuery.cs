using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Queries
{
    public record GetPrediosQuery(
        string? Busqueda = null,
        bool? EstadoActivo = null,
        TipoPredio? TipoPredio = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<PredioDto>>;

    public class GetPrediosQueryHandler : IRequestHandler<GetPrediosQuery, ResultadoPaginadoDto<PredioDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPrediosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<PredioDto>> Handle(GetPrediosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Predios
                .Include(p => p.Dominios).ThenInclude(d => d.Propietario)
                .Include(p => p.Dominios).ThenInclude(d => d.TipoTenencia)
                .Include(p => p.Bloques).ThenInclude(b => b.TipoEstructura)
                .Include(p => p.Bloques).ThenInclude(b => b.EstadoConservacion)
                .AsNoTracking()
                .AsQueryable();

            if (request.EstadoActivo.HasValue)
            {
                query = query.Where(p => p.EstadoActivo == request.EstadoActivo.Value);
            }

            if (request.TipoPredio.HasValue)
            {
                query = query.Where(p => p.TipoPredio == request.TipoPredio.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var termino = request.Busqueda.Trim().ToLower();
                query = query.Where(p =>
                    EF.Property<string>(p, "ClaveCatastral").ToLower().Contains(termino) ||
                    (p.ClaveAnterior != null && p.ClaveAnterior.ToLower().Contains(termino)) ||
                    p.Direccion.ToLower().Contains(termino));
            }

            int totalRegistros = await query.CountAsync(cancellationToken);
            int pagina = request.Pagina < 1 ? 1 : request.Pagina;
            int registrosPorPagina = request.RegistrosPorPagina < 1 ? 10 : request.RegistrosPorPagina;

            var items = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(p => new PredioDto
                {
                    Id = p.Id,
                    ClaveCatastral = p.ClaveCatastral.Valor,
                    ClaveAnterior = p.ClaveAnterior,
                    TipoPredio = p.TipoPredio,
                    AreaTerrenoEscritura = p.AreaTerrenoEscritura,
                    AreaTerrenoGrafica = p.AreaTerrenoGrafica,
                    PoligonoWkt = p.PoligonoEspacial != null ? p.PoligonoEspacial.ToText() : null,
                    Direccion = p.Direccion,
                    EstadoActivo = p.EstadoActivo,
                    Dominios = p.Dominios.Where(d => d.EstadoActivo).Select(d => new DominioDto
                    {
                        Id = d.Id,
                        PredioId = d.PredioId,
                        PropietarioId = d.PropietarioId,
                        IdentificacionPropietario = d.Propietario != null ? d.Propietario.Identificacion : string.Empty,
                        NombrePropietario = d.Propietario != null ? d.Propietario.ObtenerNombreCompleto() : string.Empty,
                        TipoTenenciaId = d.TipoTenenciaId,
                        NombreTipoTenencia = d.TipoTenencia != null ? d.TipoTenencia.Nombre : string.Empty,
                        PorcentajePropiedad = d.PorcentajePropiedad,
                        FechaInscripcion = d.FechaInscripcion,
                        Notaria = d.Notaria,
                        EstadoActivo = d.EstadoActivo
                    }).ToList(),
                    Bloques = p.Bloques.Where(b => b.EstadoActivo).Select(b => new BloqueConstruccionDto
                    {
                        Id = b.Id,
                        PredioId = b.PredioId,
                        NumeroBloque = b.NumeroBloque,
                        TipoEstructuraId = b.TipoEstructuraId,
                        NombreTipoEstructura = b.TipoEstructura != null ? b.TipoEstructura.Nombre : string.Empty,
                        EstadoConservacionId = b.EstadoConservacionId,
                        NombreEstadoConservacion = b.EstadoConservacion != null ? b.EstadoConservacion.Nombre : string.Empty,
                        NumeroPisos = b.NumeroPisos,
                        AreaConstruccion = b.AreaConstruccion,
                        AnioConstruccion = b.AnioConstruccion,
                        EstadoActivo = b.EstadoActivo
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return ResultadoPaginadoDto<PredioDto>.Crear(items, totalRegistros, pagina, registrosPorPagina);
        }
    }
}
