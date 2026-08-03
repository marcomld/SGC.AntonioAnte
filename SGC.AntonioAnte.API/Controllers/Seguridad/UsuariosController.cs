using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common;
using SGC.AntonioAnte.Application.Seguridad.Commands.Claims;
using SGC.AntonioAnte.Application.Seguridad.Commands.CreateUsuario;
using SGC.AntonioAnte.Application.Seguridad.Commands.ForgotPassword;
using SGC.AntonioAnte.Application.Seguridad.Commands.Login;
using SGC.AntonioAnte.Application.Seguridad.Commands.Logout;
using SGC.AntonioAnte.Application.Seguridad.Commands.RefreshToken;
using SGC.AntonioAnte.Application.Seguridad.Commands.ResetPassword;
using SGC.AntonioAnte.Application.Seguridad.Queries;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [ApiController]
    [Route("api/v1/seguridad/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<Usuario> _userManager;

        public UsuariosController(IMediator mediator, UserManager<Usuario> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "AdminSistemas")] // Protegido bajo las políticas de auditoría del GAD
        public async Task<IActionResult> ObtenerTodosLosFuncionarios()
        {
            // Invocamos al Query de MediatR encargado de consultar la base de datos
            var query = new ObtenerTodosUsuariosQuery();
            var resultado = await _mediator.Send(query);

            if (resultado.IsSuccess)
            {
                return Ok(new { data = resultado.Value, mensaje = "Nómina recuperada" });
            }

            return BadRequest(new { mensaje = "No se pudo consultar la lista de funcionarios" });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioDto dto)
        {
            try
            {
                var command = new CreateUsuarioCommand
                {
                    Identificacion = dto.Identificacion,
                    Nombres = dto.Nombres,
                    Apellidos = dto.Apellidos,
                    Email = dto.Email,
                    Departamento = dto.Departamento,
                    Password = dto.Password,
                    RolAsignado = dto.RolAsignado
                };

                var nuevoUsuarioId = await _mediator.Send(command);
                return Ok(new { Id = nuevoUsuarioId, Mensaje = "Funcionario registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var command = new LoginCommand
            {
                Identificacion = dto.Identificacion,
                Password = dto.Password
            };

            var tokens = await _mediator.Send(command);
            return Ok(new
            {
                Data = tokens,
                Mensaje = "Inicio de sesión exitoso."
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var command = new RefreshTokenCommand
            {
                AccessToken = dto.AccessToken,
                RefreshToken = dto.RefreshToken
            };

            var tokens = await _mediator.Send(command);
            return Ok(tokens);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _mediator.Send(new LogoutCommand());
            return Ok(new { Mensaje = "Sesión cerrada correctamente." });
        }

        [HttpPost("solicitar-recuperacion")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var command = new ForgotPasswordCommand
            {
                Email = dto.Email
            };

            // Ejecuta el envío y audita internamente sin retornar el código OTP en el JSON
            await _mediator.Send(command);
            return Ok(new { Mensaje = "Código de verificación (OTP) enviado exitosamente al correo electrónico institucional." });
        }

        [HttpPost("restablecer-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var command = new ResetPasswordCommand
            {
                Email = dto.Email,
                Token = dto.Token, // El cliente Blazor mandará los 6 dígitos aquí
                NuevaPassword = dto.NuevaPassword
            };

            await _mediator.Send(command);
            return Ok(new { Mensaje = "Contraseña restablecida exitosamente." });
        }

        // TAREA 3: Endpoint de asignación de permisos buscando determinísticamente por la Cédula (Identificación)
        [HttpPost("{identificacion}/permisos")]
        public async Task<IActionResult> AsignarPermisosFuncionario([FromRoute] string identificacion, [FromBody] AddClaimDto claimDto)
        {
            // 1. Buscamos al usuario de forma determinista usando la Cédula (Identificacion)
            var usuario = await _userManager.Users.FirstOrDefaultAsync(u => u.Identificacion == identificacion && u.EstadoActivo);

            if (usuario == null)
            {
                return NotFound(new { Mensaje = "El funcionario especificado no existe o se encuentra inactivo." });
            }

            // 2. Ejecutamos el comando de MediatR pasando el ID real (Guid) del usuario de Identity
            var command = new AddClaimCommand
            {
                UsuarioId = usuario.Id,
                ClaimType = claimDto.TipoClaim,
                ClaimValue = claimDto.ValorClaim
            };

            await _mediator.Send(command);
            return Ok(new { Mensaje = "Privilegio catastral inyectado y auditado con éxito para el funcionario." });
        }
    }
}