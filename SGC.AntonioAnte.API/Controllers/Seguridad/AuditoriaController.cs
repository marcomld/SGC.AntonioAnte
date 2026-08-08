using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Application.Seguridad.Auditorias.Queries;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [Authorize(Roles = "AdminSistemas,SuperAdmin")]
    [ApiController]
    [Route("api/v1/seguridad/auditoria")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditoriaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditorias(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] Guid? usuarioId,
            [FromQuery] string? accion,
            [FromQuery] string? entidad,
            [FromQuery] string? busqueda,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 15)
        {
            var query = new GetAuditoriasQuery(fechaDesde, fechaHasta, usuarioId, accion, entidad, busqueda, pagina, registrosPorPagina);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
