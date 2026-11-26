using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.FactoresDepreciacion.Commands;
using SGC.AntonioAnte.Application.Avaluos.FactoresDepreciacion.Queries;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresDepreciacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/factores-depreciacion")]
    [ApiController]
    [Authorize]
    public class FactoresDepreciacionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FactoresDepreciacionController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetPaginado(
            [FromQuery] int? anioFiscal,
            [FromQuery] Guid? estadoConservacionId,
            [FromQuery] bool? estadoActivo,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetFactoresDepreciacionPaginadosQuery(anioFiscal, estadoConservacionId, estadoActivo, pagina, registrosPorPagina);
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFactorDepreciacionDto dto)
        {
            var resultado = await _mediator.Send(new CreateFactorDepreciacionCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFactorDepreciacionDto dto)
        {
            if (id != dto.Id) return BadRequest(OperacionResultadoDto.Fallo("Inconsistencia en los identificadores."));
            var resultado = await _mediator.Send(new UpdateFactorDepreciacionCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new CambiarEstadoFactorDepreciacionCommand(id));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje });
        }
    }
}