using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Avaluos.Simulacion.Queries;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Avaluos
{
    [Route("api/v1/avaluos/simulaciones")]
    [ApiController]
    [Authorize]
    public class SimulacionesAvaluosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SimulacionesAvaluosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/v1/avaluos/simulaciones/{predioId}/anio/{anioFiscal}
        [HttpGet("{predioId:guid}/anio/{anioFiscal:int}")]
        public async Task<IActionResult> SimularAvaluo([FromRoute] Guid predioId, [FromRoute] int anioFiscal)
        {
            var resultado = await _mediator.Send(new SimularAvaluoQuery(predioId, anioFiscal));

            if (!resultado.Exitoso)
                return BadRequest(resultado);

            // Retornamos el DTO completo con la propiedad Datos que contiene el desglose
            return Ok(resultado);
        }
    }
}