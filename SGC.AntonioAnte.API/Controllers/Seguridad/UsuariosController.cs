using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.AddClaim;
using SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.CreateUsuario;
using SGC.AntonioAnte.Application.Seguridad.Usuarios.Queries;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios;
using SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.UpdateUsuario;
using SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.CambiarEstadoUsuario;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [ApiController]
    [Route("api/v1/seguridad/usuarios")]
    [Authorize(Roles = "AdminSistemas,SuperAdmin")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var query = new ObtenerTodosUsuariosQuery();
            var resultado = await _mediator.Send(query);

            if (resultado.IsSuccess)
            {
                return Ok(new { data = resultado.Value, mensaje = "Nómina de funcionarios recuperada." });
            }

            return BadRequest(new { mensaje = "No se pudo consultar la nómina." });
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] CreateUsuarioDto dto)
        {
            var command = new CreateUsuarioCommand
            {
                Identificacion = dto.Identificacion,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Email = dto.Email,
                DepartamentoId = dto.DepartamentoId,
                Password = dto.Password,
                RolAsignado = dto.RolAsignado
            };

            var nuevoUsuarioId = await _mediator.Send(command);
            return Ok(new { Id = nuevoUsuarioId, Mensaje = "Funcionario registrado exitosamente." });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Actualizar([FromRoute] Guid id, [FromBody] UpdateUsuarioDto dto)
        {
            var command = new UpdateUsuarioCommand
            {
                Id = id,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Email = dto.Email,
                DepartamentoId = dto.DepartamentoId
            };

            await _mediator.Send(command);
            return Ok(new { Mensaje = "Datos del funcionario actualizados exitosamente." });
        }

        [HttpPut("{id:guid}/activar")]
        public async Task<IActionResult> Activar([FromRoute] Guid id)
        {
            var command = new CambiarEstadoUsuarioCommand { Id = id, EstadoActivo = true };
            await _mediator.Send(command);
            return Ok(new { Mensaje = "Funcionario activado exitosamente." });
        }

        [HttpPut("{id:guid}/desactivar")]
        public async Task<IActionResult> Desactivar([FromRoute] Guid id)
        {
            var command = new CambiarEstadoUsuarioCommand { Id = id, EstadoActivo = false };
            await _mediator.Send(command);
            return Ok(new { Mensaje = "Funcionario desactivado exitosamente (Soft Delete)." });
        }

        // CIBERSEGURIDAD: Uso exclusivo de GUID en lugar de Cédula (Prevención IDOR / OWASP A01:2021)
        [HttpPost("{id:guid}/claims")]
        public async Task<IActionResult> AsignarClaim([FromRoute] Guid id, [FromBody] AddClaimDto claimDto)
        {
            var command = new AddClaimCommand
            {
                UsuarioId = id,
                ClaimType = claimDto.TipoClaim,
                ClaimValue = claimDto.ValorClaim
            };

            await _mediator.Send(command);
            return Ok(new { Mensaje = "Privilegio catastral inyectado y auditado con éxito para el funcionario." });
        }
    }
}