using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Catastro.Predios.Commands;
using SGC.AntonioAnte.Application.Catastro.Predios.Queries;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;

namespace SGC.AntonioAnte.API.Controllers.Catastro
{
    [ApiController]
    [Route("api/v1/catastro/predios")]
    [Authorize]
    public class PrediosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrediosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPredios(
            [FromQuery] string? busqueda,
            [FromQuery] bool? estadoActivo,
            [FromQuery] TipoPredio? tipoPredio,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetPrediosQuery(busqueda, estadoActivo, tipoPredio, pagina, registrosPorPagina);
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPredioById([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new GetPredioByIdQuery(id));

            if (resultado == null)
                return NotFound(new { mensaje = "El predio solicitado no existe o se encuentra inactivo." });

            return Ok(new { data = resultado, mensaje = "Ficha catastral recuperada exitosamente." });
        }

        // 🔥 NUEVO ENDPOINT ATÓMICO 🔥
        [HttpPost]
        public async Task<IActionResult> CreateFichaCatastral([FromBody] CreateFichaCatastralDto dto)
        {
            var command = new CreateFichaCatastralCommand(dto);
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        // 🔥 NUEVO ENDPOINT PARA EDICIÓN INTEGRAL 🔥
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFichaCatastral([FromRoute] Guid id, [FromBody] UpdateFichaCatastralDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el de la ficha." });

            var command = new UpdateFichaCatastralCommand(dto);
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new CambiarEstadoPredioCommand(id));

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }
    }
}
