using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditoria;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auditorias.Queries
{
    // CONTRATO UNIFICADO (QUERY)
    public record GetAuditoriasQuery(
        DateTime? Desde = null,
        DateTime? Hasta = null,
        Guid? UsuarioId = null,
        string? Accion = null,
        string? Entidad = null,
        string? Busqueda = null,
        int Pagina = 1,
        int RegistrosPorPagina = 15
    ) : IRequest<ResultadoPaginadoAuditDto>;

    // LÓGICA (HANDLER)
    public class GetAuditoriasQueryHandler : IRequestHandler<GetAuditoriasQuery, ResultadoPaginadoAuditDto>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditoriasQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoAuditDto> Handle(GetAuditoriasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Auditorias
                .Include(a => a.Usuario)
                .AsNoTracking()
                .AsQueryable();

            if (request.Desde.HasValue)
                query = query.Where(a => a.FechaCreacion >= request.Desde.Value.Date);

            if (request.Hasta.HasValue)
                query = query.Where(a => a.FechaCreacion <= request.Hasta.Value.Date.AddDays(1).AddTicks(-1));

            if (request.UsuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == request.UsuarioId.Value);

            if (!string.IsNullOrWhiteSpace(request.Accion))
                query = query.Where(a => a.Accion == request.Accion);

            if (!string.IsNullOrWhiteSpace(request.Entidad))
                query = query.Where(a => a.Entidad == request.Entidad);

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                string term = request.Busqueda.Trim().ToLower();

                query = query.Where(a => a.Accion.ToLower().Contains(term) ||
                                         a.Entidad.ToLower().Contains(term) ||
                                         a.DireccionIp.Contains(term) ||
                                         (a.Usuario != null && ((a.Usuario.Nombres != null && a.Usuario.Nombres.ToLower().Contains(term)) ||
                                                                (a.Usuario.Apellidos != null && a.Usuario.Apellidos.ToLower().Contains(term)) ||
                                                                (a.Usuario.Identificacion != null && a.Usuario.Identificacion.Contains(term)) ||
                                                                (a.Usuario.Email != null && a.Usuario.Email.ToLower().Contains(term)))));
            }

            int totalRegistros = await query.CountAsync(cancellationToken);

            int paginaAjustada = request.Pagina < 1 ? 1 : request.Pagina;
            int tamañoAjustado = request.RegistrosPorPagina < 5 ? 15 : request.RegistrosPorPagina;

            var items = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Skip((paginaAjustada - 1) * tamañoAjustado)
                .Take(tamañoAjustado)
                .Select(a => new AuditLogResponseDto
                {
                    Id = a.Id,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = a.Usuario != null ? $"{a.Usuario.Nombres} {a.Usuario.Apellidos}" : "Sistema / Proceso Automático",

                    EmailUsuario = a.Usuario != null && a.Usuario.Email != null ? a.Usuario.Email : string.Empty,
                    IdentificacionUsuario = a.Usuario != null && a.Usuario.Identificacion != null ? a.Usuario.Identificacion : string.Empty,

                    Accion = a.Accion,
                    Entidad = a.Entidad,
                    EntidadId = a.EntidadId,
                    DireccionIp = a.DireccionIp,
                    Navegador = a.Navegador,
                    DatosAdicionales = a.DatosAdicionales ?? string.Empty,
                    FechaCreacion = a.FechaCreacion
                })
                .ToListAsync(cancellationToken);

            return new ResultadoPaginadoAuditDto
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = paginaAjustada,
                RegistrosPorPagina = tamañoAjustado
            };
        }
    }
}