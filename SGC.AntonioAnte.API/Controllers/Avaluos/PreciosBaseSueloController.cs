using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.PreciosBaseSuelo.Commands;
using SGC.AntonioAnte.Application.Avaluos.PreciosBaseSuelo.Queries;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.PreciosBaseSuelo;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/precios-suelo")]
    [ApiController]
    [Authorize]
    public class PreciosBaseSueloController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PreciosBaseSueloController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetPaginado(
            [FromQuery] int? anioFiscal,
            [FromQuery] TipoPredio? tipoPredio,
            [FromQuery] string? terminoBusqueda,
            [FromQuery] bool? estadoActivo,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetPreciosBaseSueloPaginadosQuery(anioFiscal, tipoPredio, terminoBusqueda, estadoActivo, pagina, registrosPorPagina);
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePrecioBaseSueloDto dto)
        {
            var resultado = await _mediator.Send(new CreatePrecioBaseSueloCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePrecioBaseSueloDto dto)
        {
            if (id != dto.Id) return BadRequest(OperacionResultadoDto.Fallo("Inconsistencia en los identificadores."));
            var resultado = await _mediator.Send(new UpdatePrecioBaseSueloCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new CambiarEstadoPrecioBaseSueloCommand(id));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje });
        }
    }
}