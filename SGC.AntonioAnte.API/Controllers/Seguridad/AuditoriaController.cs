using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditoria;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [ApiController]
    [Route("api/v1/seguridad/auditoria")]
    [Authorize(Roles = "AdminSistemas,SuperAdmin")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        public AuditoriaController(IApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ConsultarBitacora(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] Guid? usuarioId,
            [FromQuery] string? accion,
            [FromQuery] string? entidad,
            [FromQuery] string? busqueda,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 15)
        {
            var query = _context.Auditorias
                .Include(a => a.Usuario)
                .AsNoTracking()
                .AsQueryable();

            if (desde.HasValue)
                query = query.Where(a => a.FechaCreacion >= desde.Value.Date);

            if (hasta.HasValue)
                query = query.Where(a => a.FechaCreacion <= hasta.Value.Date.AddDays(1).AddTicks(-1));

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(a => a.Accion == accion);

            if (!string.IsNullOrWhiteSpace(entidad))
                query = query.Where(a => a.Entidad == entidad);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string term = busqueda.Trim().ToLower();
                query = query.Where(a => a.Accion.ToLower().Contains(term) ||
                                         a.Entidad.ToLower().Contains(term) ||
                                         a.DireccionIp.Contains(term) ||
                                         (a.Usuario != null && (a.Usuario.Nombres.ToLower().Contains(term) ||
                                                                a.Usuario.Apellidos.ToLower().Contains(term) ||
                                                                a.Usuario.Identificacion.Contains(term) ||
                                                                a.Usuario.Email.ToLower().Contains(term))));
            }

            int totalRegistros = await query.CountAsync();

            int paginaAjustada = pagina < 1 ? 1 : pagina;
            int tamañoAjustado = registrosPorPagina < 5 ? 15 : registrosPorPagina;

            var items = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Skip((paginaAjustada - 1) * tamañoAjustado)
                .Take(tamañoAjustado)
                .Select(a => new AuditLogResponseDto
                {
                    Id = a.Id,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = a.Usuario != null ? $"{a.Usuario.Nombres} {a.Usuario.Apellidos}" : "Sistema / Proceso Automático",
                    EmailUsuario = a.Usuario != null ? a.Usuario.Email : string.Empty,
                    IdentificacionUsuario = a.Usuario != null ? a.Usuario.Identificacion : string.Empty,
                    Accion = a.Accion,
                    Entidad = a.Entidad,
                    EntidadId = a.EntidadId,
                    DireccionIp = a.DireccionIp,
                    Navegador = a.Navegador,
                    DatosAdicionales = a.DatosAdicionales ?? string.Empty,
                    FechaCreacion = a.FechaCreacion
                })
                .ToListAsync();

            var resultado = new ResultadoPaginadoAuditDto
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = paginaAjustada,
                RegistrosPorPagina = tamañoAjustado
            };

            return Ok(new { data = resultado, mensaje = "Bitácora consultada correctamente." });
        }
    }
}
