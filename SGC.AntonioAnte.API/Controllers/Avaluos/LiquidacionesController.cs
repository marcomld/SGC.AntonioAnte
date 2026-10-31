using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.Liquidaciones.Commands;
using SGC.AntonioAnte.Application.Avaluos.Liquidaciones.Queries;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/liquidaciones")]
    [ApiController]
    [Authorize]
    public class LiquidacionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LiquidacionesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("emitir")]
        public async Task<IActionResult> EmitirLiquidacion([FromBody] EmitirLiquidacionDto dto)
        {
            var resultado = await _mediator.Send(new EmitirLiquidacionCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }

        // 🔹 Nuevo: GET Consultar la liquidación oficial vigente
        [HttpGet("predio/{predioId:guid}/anio/{anioFiscal:int}")]
        public async Task<IActionResult> GetVigente([FromRoute] Guid predioId, [FromRoute] int anioFiscal)
        {
            var resultado = await _mediator.Send(new GetLiquidacionVigenteQuery(predioId, anioFiscal));
            if (!resultado.Exitoso) return NotFound(new { mensaje = resultado.Mensaje });
            return Ok(resultado);
        }

        // 🔹 Nuevo: POST Reliquidar liquidación
        [HttpPost("reliquidar")]
        public async Task<IActionResult> Reliquidar([FromBody] ReliquidarLiquidacionDto dto)
        {
            var resultado = await _mediator.Send(new ReliquidarLiquidacionCommand(dto));
            if (!resultado.Exitoso) return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }
    }
}