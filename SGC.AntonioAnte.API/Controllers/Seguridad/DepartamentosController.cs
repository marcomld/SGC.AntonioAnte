using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.CambiarEstadoDepartamento;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.CreateDepartamento;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.DeleteDepartamento;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.UpdateDepartamento;
using SGC.AntonioAnte.Application.Seguridad.Departamentos.Queries;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [ApiController]
    [Route("api/v1/seguridad/departamentos")]
    [Authorize(Roles = "AdminSistemas,SuperAdmin")]
    public class DepartamentosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepartamentosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var departamentos = await _mediator.Send(new ObtenerTodosDepartamentosQuery());
            return Ok(new { data = departamentos, mensaje = "Catálogo de departamentos recuperado." });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CreateDepartamentoDto dto)
        {
            var command = new CreateDepartamentoCommand { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            var id = await _mediator.Send(command);
            return Ok(new { id, mensaje = "Departamento registrado exitosamente." });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Actualizar([FromRoute] Guid id, [FromBody] CreateDepartamentoDto dto)
        {
            var command = new UpdateDepartamentoCommand { Id = id, Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Departamento actualizado correctamente." });
        }

        [HttpPut("{id:guid}/toggle-status")]
        public async Task<IActionResult> CambiarEstado([FromRoute] Guid id)
        {
            var command = new CambiarEstadoDepartamentoCommand { Id = id };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Estado del departamento modificado exitosamente." });
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