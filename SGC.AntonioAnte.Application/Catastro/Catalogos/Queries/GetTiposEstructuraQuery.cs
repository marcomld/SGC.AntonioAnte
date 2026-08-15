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
    public record GetTiposEstructuraQuery : IRequest<List<CatalogoDto>>;

    public class ObtenerTiposEstructuraQueryHandler : IRequestHandler<GetTiposEstructuraQuery, List<CatalogoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerTiposEstructuraQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CatalogoDto>> Handle(GetTiposEstructuraQuery request, CancellationToken cancellationToken)
        {
            return await _context.TiposEstructura
                .Where(t => t.EstadoActivo)
                .OrderBy(t => t.Nombre)
                .Select(t => new CatalogoDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    EstadoActivo = t.EstadoActivo
                })
                .ToListAsync(cancellationToken);
        }
    }
}
