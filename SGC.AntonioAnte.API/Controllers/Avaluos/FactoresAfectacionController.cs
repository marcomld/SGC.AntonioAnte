using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.FactoresAfectacion.Commands;
using SGC.AntonioAnte.Application.Avaluos.FactoresAfectacion.Queries;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresAfectacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/factores-afectacion")]
    [ApiController]
    [Authorize]
    public class FactoresAfectacionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FactoresAfectacionController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetPaginado(
            [FromQuery] int? anioFiscal,
            [FromQuery] string? terminoBusqueda,
            [FromQuery] bool? estadoActivo,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetFactoresAfectacionPaginadosQuery(anioFiscal, terminoBusqueda, estadoActivo, pagina, registrosPorPagina);
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFactorAfectacionDto dto)
        {
            var resultado = await _mediator.Send(new CreateFactorAfectacionCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFactorAfectacionDto dto)
        {
            if (id != dto.Id) return BadRequest(OperacionResultadoDto.Fallo("Inconsistencia en los identificadores."));
            var resultado = await _mediator.Send(new UpdateFactorAfectacionCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new CambiarEstadoFactorAfectacionCommand(id));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje });
        }
    }
}