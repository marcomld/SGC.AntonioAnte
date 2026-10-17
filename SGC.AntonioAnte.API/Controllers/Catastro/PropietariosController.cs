using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Catastro.Propietarios.Commands;
using SGC.AntonioAnte.Application.Catastro.Propietarios.Queries;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Catastro
{
    [ApiController]
    [Route("api/v1/catastro/propietarios")]
    [Authorize]
    public class PropietariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PropietariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPropietarios(
            [FromQuery] string? busqueda,
            [FromQuery] bool? estadoActivo,
            [FromQuery] TipoPropietario? tipoPropietario,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetPropietariosQuery(busqueda, estadoActivo, tipoPropietario, pagina, registrosPorPagina);
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }

        [HttpGet("{identificacion}")]
        public async Task<IActionResult> GetByIdentificacion([FromRoute] string identificacion)
        {
            var query = new GetPropietarioByIdentificacionQuery(identificacion);
            var resultado = await _mediator.Send(query);

            if (resultado == null)
                return NotFound(new { mensaje = "No se encontró ningún propietario con la identificación especificada." });

            return Ok(new { data = resultado, mensaje = "Propietario recuperado exitosamente." });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePropietarioDto dto)
        {
            var command = new CreatePropietarioCommand(
                dto.TipoPropietario,
                dto.Identificacion,
                dto.Nombres,
                dto.Apellidos,
                dto.RazonSocial,
                dto.EstadoCivil,
                dto.Email,
                dto.Telefono
            );

            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePropietarioDto dto)
        {
            var command = new UpdatePropietarioCommand(
                id,
                dto.Nombres,
                dto.Apellidos,
                dto.RazonSocial,
                dto.EstadoCivil,
                dto.Email,
                dto.Telefono
            );

            var exitoso = await _mediator.Send(command);
            return Ok(new { exitoso, mensaje = "Propietario actualizado exitosamente." });
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new CambiarEstadoPropietarioCommand(id));

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }
    }
}