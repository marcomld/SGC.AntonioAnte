using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Catastro.Predios.Commands;
using SGC.AntonioAnte.Application.Catastro.Predios.Queries;
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
        public async Task<IActionResult> GetPredios([FromQuery] string? busqueda)
        {
            var resultado = await _mediator.Send(new GetPrediosQuery(busqueda));
            return Ok(new { data = resultado, mensaje = "Listado de predios recuperado exitosamente." });
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

            var id = await _mediator.Send(command);
            return Ok(new { id, mensaje = "Predio (terreno base) registrado exitosamente en el catastro municipal." });
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
    }
}
