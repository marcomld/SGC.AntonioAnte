using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Catalogos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Catalogos.Queries
{
    public record GetEstadosConservacionQuery : IRequest<List<CatalogoDto>>;

    public class ObtenerEstadosConservacionQueryHandler : IRequestHandler<GetEstadosConservacionQuery, List<CatalogoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerEstadosConservacionQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CatalogoDto>> Handle(GetEstadosConservacionQuery request, CancellationToken cancellationToken)
        {
            return await _context.EstadosConservacion
                .Where(e => e.EstadoActivo)
                .OrderBy(e => e.Nombre)
                .Select(e => new CatalogoDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    EstadoActivo = e.EstadoActivo
                })
                .ToListAsync(cancellationToken);
        }
    }
}
