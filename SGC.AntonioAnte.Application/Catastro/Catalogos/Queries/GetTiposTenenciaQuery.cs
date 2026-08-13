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
    public record GetTiposTenenciaQuery : IRequest<List<CatalogoDto>>;

    public class ObtenerTiposTenenciaQueryHandler : IRequestHandler<GetTiposTenenciaQuery, List<CatalogoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerTiposTenenciaQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CatalogoDto>> Handle(GetTiposTenenciaQuery request, CancellationToken cancellationToken)
        {
            return await _context.TiposTenencia
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
