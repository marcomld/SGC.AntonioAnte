using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Queries
{
    // QUERY
    public record ObtenerTodosDepartamentosQuery : IRequest<List<DepartamentoDto>>;

    // HANDLER
    public class ObtenerTodosDepartamentosQueryHandler : IRequestHandler<ObtenerTodosDepartamentosQuery, List<DepartamentoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerTodosDepartamentosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartamentoDto>> Handle(ObtenerTodosDepartamentosQuery request, CancellationToken cancellationToken)
        {
            return await _context.Departamentos
                .AsNoTracking()
                .Select(d => new DepartamentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion ?? string.Empty,
                    EstadoActivo = d.EstadoActivo
                })
                .ToListAsync(cancellationToken);
        }
    }
}
