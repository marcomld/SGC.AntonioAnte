using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Queries
{
    // 1. QUERY
    public record GetDepartamentosQuery(
        string? Busqueda = null,
        bool? EstadoActivo = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<DepartamentoDto>>;

    // 2. HANDLER
    public class GetDepartamentosQueryHandler : IRequestHandler<GetDepartamentosQuery, ResultadoPaginadoDto<DepartamentoDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDepartamentosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<DepartamentoDto>> Handle(GetDepartamentosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Departamentos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var busquedaNorm = request.Busqueda.Trim().ToLower();
                query = query.Where(d =>
                    d.Nombre.ToLower().Contains(busquedaNorm) ||
                    (d.Descripcion != null && d.Descripcion.ToLower().Contains(busquedaNorm)));
            }

            if (request.EstadoActivo.HasValue)
            {
                query = query.Where(d => d.EstadoActivo == request.EstadoActivo.Value);
            }

            var totalRegistros = await query.CountAsync(cancellationToken);

            int pagina = request.Pagina < 1 ? 1 : request.Pagina;
            int registrosPorPagina = request.RegistrosPorPagina < 1 ? 10 : request.RegistrosPorPagina;

            var items = await query
                .OrderBy(d => d.Nombre)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(d => new DepartamentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion ?? string.Empty,
                    EstadoActivo = d.EstadoActivo
                })
                .ToListAsync(cancellationToken);

            return ResultadoPaginadoDto<DepartamentoDto>.Crear(
                items,
                totalRegistros,
                pagina,
                registrosPorPagina);
        }
    }
}