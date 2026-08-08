using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.DeleteDepartamento;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Queries;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{

    [Authorize]
    [ApiController]
    [Route("api/v1/seguridad/departamentos")]
    public class DepartamentosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepartamentosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] string? busqueda,
            [FromQuery] bool? estadoActivo,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = new GetDepartamentosQuery(busqueda, estadoActivo, pagina, registrosPorPagina);
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CreateDepartamentoDto dto)
        {
            var command = new CreateDepartamentoCommand { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Actualizar([FromRoute] Guid id, [FromBody] CreateDepartamentoDto dto)
        {
            var command = new UpdateDepartamentoCommand { Id = id, Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpPut("{id:guid}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromRoute] Guid id)
        {
            var command = new CambiarEstadoDepartamentoCommand { Id = id };
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar([FromRoute] Guid id)
        {
            var command = new DeleteDepartamentoCommand { Id = id };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Departamento eliminado permanentemente." });
        }
    }
}