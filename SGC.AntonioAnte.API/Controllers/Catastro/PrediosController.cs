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
            {
                return NotFound(new { mensaje = "El predio solicitado no existe o se encuentra inactivo." });
            }

            return Ok(new { data = resultado, mensaje = "Ficha catastral recuperada exitosamente." });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePredio([FromBody] CreatePredioDto dto)
        {
            var command = new CreatePredioCommand(
                dto.ClaveCatastral,
                dto.ClaveAnterior,
                dto.TipoPredio,
                dto.AreaTerrenoEscritura,
                dto.AreaTerrenoGrafica,
                dto.Direccion,
                dto.PoligonoWkt
            );

            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpPost("{id:guid}/dominios")]
        public async Task<IActionResult> AddDominio([FromRoute] Guid id, [FromBody] AddDominioDto dto)
        {
            var command = new AddDominioCommand(
                id,
                dto.PropietarioId,
                dto.TipoTenenciaId,
                dto.PorcentajePropiedad,
                dto.FechaInscripcion,
                dto.Notaria
            );

            var dominioId = await _mediator.Send(command);
            return Ok(new { id = dominioId, mensaje = "Derecho de dominio y propietario asignado exitosamente al predio." });
        }

        [HttpPost("{id:guid}/bloques")]
        public async Task<IActionResult> AddBloque([FromRoute] Guid id, [FromBody] AddBloqueDto dto)
        {
            var command = new AddBloqueCommand(
                id,
                dto.NumeroBloque,
                dto.TipoEstructuraId,
                dto.EstadoConservacionId,
                dto.NumeroPisos,
                dto.AreaConstruccion,
                dto.AnioConstruccion
            );

            var bloqueId = await _mediator.Send(command);
            return Ok(new { id = bloqueId, mensaje = "Bloque de construcción registrado exitosamente en el predio." });
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
