using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.CostosReposicion.Commands;
using SGC.AntonioAnte.Application.Avaluos.CostosReposicion.Queries;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/costos-reposicion")]
    [ApiController]
    [Authorize]
    public class CostosReposicionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CostosReposicionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaginado(
            [FromQuery] int? anioFiscal,
            [FromQuery] Guid? tipoEstructuraId,
            [FromQuery] bool? estadoActivo,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetCostosReposicionPaginadosQuery(anioFiscal, tipoEstructuraId, estadoActivo, pagina, registrosPorPagina);
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCostoReposicionDto dto)
        {
            var resultado = await _mediator.Send(new CreateCostoReposicionCommand(dto));

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCostoReposicionDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { mensaje = "Inconsistencia en los identificadores." });

            var resultado = await _mediator.Send(new UpdateCostoReposicionCommand(dto));

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new CambiarEstadoCostoReposicionCommand(id));

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }
    }
}