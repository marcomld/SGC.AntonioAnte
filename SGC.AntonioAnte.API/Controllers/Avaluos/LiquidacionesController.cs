using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.Liquidaciones.Commands;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/liquidaciones")]
    [ApiController]
    [Authorize]
    public class LiquidacionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LiquidacionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/v1/avaluos/liquidaciones/emitir
        [HttpPost("emitir")]
        public async Task<IActionResult> EmitirLiquidacion([FromBody] EmitirLiquidacionDto dto)
        {
            var resultado = await _mediator.Send(new EmitirLiquidacionCommand(dto));

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje, id = resultado.Id });
        }
    }
}
