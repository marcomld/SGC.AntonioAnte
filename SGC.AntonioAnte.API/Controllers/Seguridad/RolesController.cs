using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Seguridad.Roles.Command.AsignarPermisosGranulares;
using SGC.AntonioAnte.Application.Seguridad.Roles.Command.AsignarPermisosRol;
using SGC.AntonioAnte.Application.Seguridad.Roles.Commands;
using SGC.AntonioAnte.Application.Seguridad.Roles.Commands.AsignarRolUsuario;
using SGC.AntonioAnte.Application.Seguridad.Roles.Commands.DesasignarRolUsuario;
using SGC.AntonioAnte.Application.Seguridad.Roles.Queries;

using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [ApiController]
    [Route("api/v1/seguridad/roles")]
    [Authorize(Roles = "AdminSistemas,SuperAdmin")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _mediator.Send(new GetRolesQuery());
            return Ok(new { data = roles, mensaje = "Catálogo de roles recuperado exitosamente." });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CreateRoleDto dto)
        {
            var command = new CreateRolCommand(dto.Nombre, dto.Descripcion);
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar([FromRoute] Guid id)
        {
            var command = new DeleteRolCommand(id);
            var resultado = await _mediator.Send(command);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpPost("asignar-usuario")]
        public async Task<IActionResult> AsignarRolUsuario([FromBody] AssignRoleDto dto)
        {
            var command = new AsignarRolUsuarioCommand { UsuarioId = dto.UsuarioId, NombreRol = dto.NombreRol };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Rol asignado correctamente al funcionario." });
        }

        [HttpPost("desasignar-usuario")]
        public async Task<IActionResult> DesasignarRolUsuario([FromBody] AssignRoleDto dto)
        {
            var command = new DesasignarRolUsuarioCommand { UsuarioId = dto.UsuarioId, NombreRol = dto.NombreRol };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Rol removido correctamente del funcionario." });
        }

        // GET: api/v1/seguridad/roles/{rolId}/permisos (Para Roles.razor)
        [HttpGet("{rolId:guid}/permisos")]
        public async Task<IActionResult> ObtenerPermisosRol([FromRoute] Guid rolId)
        {
            var permisos = await _mediator.Send(new ObtenerPermisosRolQuery(rolId));
            return Ok(new { data = permisos, mensaje = "Matriz de permisos del rol recuperada." });
        }

        // POST: api/v1/seguridad/roles/permisos (Para Roles.razor)
        [HttpPost("permisos")]
        public async Task<IActionResult> AsignarPermisosRol([FromBody] RolePermissionDto dto)
        {
            var command = new AsignarPermisosRolCommand
            {
                RolId = dto.RolId,
                Permisos = dto.Permisos
            };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Permisos del rol actualizados correctamente." });
        }

        // GET: api/v1/seguridad/roles/usuarios/{usuarioId}/permisos (Para Usuarios.razor)
        [HttpGet("usuarios/{usuarioId:guid}/permisos")]
        public async Task<IActionResult> ObtenerPermisosUsuario([FromRoute] Guid usuarioId)
        {
            var permisos = await _mediator.Send(new ObtenerPermisosUsuarioQuery(usuarioId));
            return Ok(new { data = permisos, mensaje = "Permisos del funcionario recuperados." });
        }

        // POST: api/v1/seguridad/roles/usuarios/permisos (Para Usuarios.razor)
        [HttpPost("usuarios/permisos")]
        public async Task<IActionResult> AsignarPermisosUsuario([FromBody] RolePermissionDto dto)
        {
            var command = new AsignarPermisosGranularesCommand
            {
                UsuarioId = dto.RolId,
                Permisos = dto.Permisos
            };
            await _mediator.Send(command);
            return Ok(new { mensaje = "Permisos especiales del funcionario actualizados." });
        }
    }
}