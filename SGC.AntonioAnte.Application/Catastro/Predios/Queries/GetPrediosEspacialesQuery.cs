using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Queries
{
    public record GetPrediosEspacialesQuery(
        string? Busqueda = null,
        TipoPredio? TipoPredio = null
    ) : IRequest<List<PredioEspacialDto>>;

    public class GetPrediosEspacialesQueryHandler : IRequestHandler<GetPrediosEspacialesQuery, List<PredioEspacialDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPrediosEspacialesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PredioEspacialDto>> Handle(GetPrediosEspacialesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Predios
                .AsNoTracking()
                .Include(p => p.Dominios).ThenInclude(d => d.Propietario)
                .Include(p => p.Bloques)
                .Where(p => p.EstadoActivo && p.PoligonoEspacial != null)
                .AsQueryable();

            if (request.TipoPredio.HasValue)
            {
                query = query.Where(p => p.TipoPredio == request.TipoPredio.Value);
            }

            // Proyección optimizada desde SQL Server
            var prediosCrudos = await query.ToListAsync(cancellationToken);

            var resultado = prediosCrudos.Select(p =>
            {
                var dominioPrincipal = p.Dominios
                    .Where(d => d.EstadoActivo && d.Propietario != null)
                    .OrderByDescending(d => d.PorcentajePropiedad)
                    .FirstOrDefault();

                return new PredioEspacialDto
                {
                    Id = p.Id,
                    ClaveCatastral = p.ClaveCatastral.Valor,
                    ClaveAnterior = p.ClaveAnterior,
                    TipoPredio = p.TipoPredio,
                    Direccion = p.Direccion,
                    AreaTerrenoEscritura = p.AreaTerrenoEscritura,
                    AreaTerrenoGrafica = p.AreaTerrenoGrafica,
                    PoligonoWkt = p.PoligonoEspacial!.ToText(),
                    PropietarioPrincipal = dominioPrincipal?.Propietario?.ObtenerNombreCompleto() ?? "Sin propietario asignado",
                    TotalBloques = p.Bloques.Count(b => b.EstadoActivo)
                };
            }).ToList();

            // Filtrado resiliente en memoria para sortear la restricción del Value Object
            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var termino = request.Busqueda.Trim().ToLower();
                resultado = resultado.Where(p =>
                    p.ClaveCatastral.Contains(termino) ||
                    (p.ClaveAnterior != null && p.ClaveAnterior.ToLower().Contains(termino)) ||
                    p.Direccion.ToLower().Contains(termino) ||
                    p.PropietarioPrincipal.ToLower().Contains(termino)
                ).ToList();
            }

            return resultado.OrderBy(p => p.ClaveCatastral).ToList();
        }
    }
}