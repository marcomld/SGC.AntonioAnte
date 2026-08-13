using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Catastro.Propietarios.Commands;
using SGC.AntonioAnte.Application.Catastro.Propietarios.Queries;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;

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
        public async Task<IActionResult> GetPropietarios([FromQuery] string? busqueda)
        {
            var resultado = await _mediator.Send(new GetPropietariosQuery(busqueda));
            return Ok(new { data = resultado, mensaje = "Listado de propietarios recuperado exitosamente." });
        }

        [HttpGet("{identificacion}")]
        public async Task<IActionResult> GetByIdentificacion([FromRoute] string identificacion)
        {
            var resultado = await _mediator.Send(new GetPropietarioByIdentificacionQuery(identificacion));

            if (resultado == null)
            {
                return NotFound(new { mensaje = $"No se encontró ningún ciudadano/empresa con la identificación '{identificacion}'." });
            }

            return Ok(new { data = resultado, mensaje = "Propietario encontrado." });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePropietario([FromBody] CreatePropietarioDto dto)
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

            var id = await _mediator.Send(command);
            return Ok(new { id, mensaje = "Sujeto de derecho (propietario) registrado exitosamente." });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePropietario([FromRoute] Guid id, [FromBody] UpdatePropietarioDto dto)
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

            await _mediator.Send(command);
            return Ok(new { mensaje = "Datos del propietario actualizados exitosamente." });
        }
    }
}
