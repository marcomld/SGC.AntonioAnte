using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Queries
{
    public record GetPrediosQuery(string? Busqueda = null) : IRequest<List<PredioDto>>;

    public class GetPrediosQueryHandler : IRequestHandler<GetPrediosQuery, List<PredioDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPrediosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PredioDto>> Handle(GetPrediosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Predios
                .Include(p => p.Dominios).ThenInclude(d => d.Propietario)
                .Include(p => p.Dominios).ThenInclude(d => d.TipoTenencia)
                .Include(p => p.Bloques).ThenInclude(b => b.TipoEstructura)
                .Include(p => p.Bloques).ThenInclude(b => b.EstadoConservacion)
                .Where(p => p.EstadoActivo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var termino = request.Busqueda.Trim().ToLower();
                // CORRECCIÓN: Usamos EF.Property<string> para buscar en la columna convertida
                query = query.Where(p =>
                    EF.Property<string>(p, "ClaveCatastral").Contains(termino) ||
                    (p.ClaveAnterior != null && p.ClaveAnterior.ToLower().Contains(termino)) ||
                    p.Direccion.ToLower().Contains(termino));
            }

            var predios = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Take(100)
                .ToListAsync(cancellationToken);

            return predios.Select(p => new PredioDto
            {
                Id = p.Id,
                ClaveCatastral = p.ClaveCatastral.Valor,
                ClaveAnterior = p.ClaveAnterior,
                TipoPredio = p.TipoPredio,
                AreaTerrenoEscritura = p.AreaTerrenoEscritura,
                AreaTerrenoGrafica = p.AreaTerrenoGrafica,
                PoligonoWkt = p.PoligonoEspacial?.ToText(),
                Direccion = p.Direccion,
                EstadoActivo = p.EstadoActivo,
                Dominios = p.Dominios.Where(d => d.EstadoActivo).Select(d => new DominioDto
                {
                    Id = d.Id,
                    PredioId = d.PredioId,
                    PropietarioId = d.PropietarioId,
                    IdentificacionPropietario = d.Propietario?.Identificacion ?? string.Empty,
                    NombrePropietario = d.Propietario?.ObtenerNombreCompleto() ?? string.Empty,
                    TipoTenenciaId = d.TipoTenenciaId,
                    NombreTipoTenencia = d.TipoTenencia?.Nombre ?? string.Empty,
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
                    NombreTipoEstructura = b.TipoEstructura?.Nombre ?? string.Empty,
                    EstadoConservacionId = b.EstadoConservacionId,
                    NombreEstadoConservacion = b.EstadoConservacion?.Nombre ?? string.Empty,
                    NumeroPisos = b.NumeroPisos,
                    AreaConstruccion = b.AreaConstruccion,
                    AnioConstruccion = b.AnioConstruccion,
                    EstadoActivo = b.EstadoActivo
                }).ToList()
            }).ToList();
        }
    }
}
