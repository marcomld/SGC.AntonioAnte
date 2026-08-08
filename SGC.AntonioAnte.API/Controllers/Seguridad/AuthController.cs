using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Seguridad.Auth.Commands;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Controllers.Seguridad
{
    [ApiController]
    [Route("api/v1/seguridad/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Instanciamos el record posicional en una línea
            var command = new LoginCommand(dto.Identificacion, dto.Password);

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
            var command = new RefreshTokenCommand(dto.AccessToken, dto.RefreshToken);
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
            var command = new ForgotPasswordCommand(dto.Email);
            await _mediator.Send(command);

            return Ok(new { Mensaje = "Código de verificación (OTP) enviado exitosamente al correo electrónico institucional." });
        }

        [HttpPost("restablecer-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var command = new ResetPasswordCommand(dto.Email, dto.Token, dto.NuevaPassword);
            await _mediator.Send(command);

            return Ok(new { Mensaje = "Contraseña restablecida exitosamente." });
        }
    }
}