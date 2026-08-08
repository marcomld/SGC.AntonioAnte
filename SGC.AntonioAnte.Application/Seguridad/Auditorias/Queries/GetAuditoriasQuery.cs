using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditorias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auditorias.Queries
{
    // 1. QUERY
    public record GetAuditoriasQuery(
        DateTime? FechaDesde = null,
        DateTime? FechaHasta = null,
        Guid? UsuarioId = null,
        string? Accion = null,
        string? Entidad = null,
        string? Busqueda = null,
        int Pagina = 1,
        int RegistrosPorPagina = 15
    ) : IRequest<ResultadoPaginadoDto<AuditLogResponseDto>>;

    // 2. HANDLER
    public class GetAuditoriasQueryHandler : IRequestHandler<GetAuditoriasQuery, ResultadoPaginadoDto<AuditLogResponseDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditoriasQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<AuditLogResponseDto>> Handle(GetAuditoriasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Auditorias
                .Include(a => a.Usuario)
                .AsNoTracking()
                .AsQueryable();

            // Filtro Fecha Desde
            if (request.FechaDesde.HasValue)
            {
                var desdeUtc = request.FechaDesde.Value.Date.ToUniversalTime();
                query = query.Where(a => a.FechaCreacion >= desdeUtc);
            }

            // Filtro Fecha Hasta
            if (request.FechaHasta.HasValue)
            {
                var hastaUtc = request.FechaHasta.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(a => a.FechaCreacion <= hastaUtc);
            }

            // Filtro Usuario
            if (request.UsuarioId.HasValue)
            {
                query = query.Where(a => a.UsuarioId == request.UsuarioId.Value);
            }

            // Filtro Acción
            if (!string.IsNullOrWhiteSpace(request.Accion))
            {
                var accionNorm = request.Accion.Trim().ToLower();
                query = query.Where(a => a.Accion.ToLower().Contains(accionNorm));
            }

            // Filtro Entidad
            if (!string.IsNullOrWhiteSpace(request.Entidad))
            {
                var entidadNorm = request.Entidad.Trim().ToLower();
                query = query.Where(a => a.Entidad.ToLower().Contains(entidadNorm));
            }

            // Búsqueda General
            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var busquedaNorm = request.Busqueda.Trim().ToLower();
                query = query.Where(a =>
                    a.Accion.ToLower().Contains(busquedaNorm) ||
                    a.Entidad.ToLower().Contains(busquedaNorm) ||
                    (a.EntidadId != null && a.EntidadId.ToLower().Contains(busquedaNorm)) ||
                    (a.DireccionIp != null && a.DireccionIp.ToLower().Contains(busquedaNorm)) ||
                    (a.Usuario != null && (
                        a.Usuario.Nombres.ToLower().Contains(busquedaNorm) ||
                        a.Usuario.Apellidos.ToLower().Contains(busquedaNorm) ||
                        a.Usuario.Identificacion.ToLower().Contains(busquedaNorm) ||
                        (a.Usuario.Email != null && a.Usuario.Email.ToLower().Contains(busquedaNorm))
                    ))
                );
            }

            var totalRegistros = await query.CountAsync(cancellationToken);

            int pagina = request.Pagina < 1 ? 1 : request.Pagina;
            int registrosPorPagina = request.RegistrosPorPagina < 1 ? 15 : request.RegistrosPorPagina;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var items = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(a => new AuditLogResponseDto
                {
                    Id = a.Id,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = a.Usuario != null ? $"{a.Usuario.Nombres} {a.Usuario.Apellidos}" : "Sistema / Proceso Automático",
                    IdentificacionUsuario = a.Usuario != null ? a.Usuario.Identificacion : string.Empty,
                    EmailUsuario = a.Usuario != null ? (a.Usuario.Email ?? string.Empty) : string.Empty,
                    Accion = a.Accion ?? string.Empty,
                    Entidad = a.Entidad ?? string.Empty,
                    EntidadId = a.EntidadId ?? string.Empty,             // 🔹 Soluciona el aviso CS8601
                    DatosAdicionales = a.DatosAdicionales ?? string.Empty, // 🔹 Soluciona el aviso CS8601
                    DireccionIp = a.DireccionIp ?? string.Empty,         // 🔹 Soluciona el aviso CS8601
                    Navegador = a.Navegador ?? string.Empty,             // 🔹 Soluciona el aviso CS8601
                    FechaCreacion = a.FechaCreacion
                })
                .ToListAsync(cancellationToken);

            return new ResultadoPaginadoDto<AuditLogResponseDto>
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = pagina,
                RegistrosPorPagina = registrosPorPagina,
                TotalPaginas = totalPaginas
            };
        }
    }
}