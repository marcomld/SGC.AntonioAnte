using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SGC.AntonioAnte.Application.Seguridad.Auth;
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            var command = new LoginCommand(
                dto.Identificacion,
                dto.Password,
                dto.Recordarme);

            var tokens = await _mediator.Send(command);

            return Ok(new
            {
                Data = tokens,
                Mensaje = "Inicio de sesión exitoso."
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenDto dto)
        {
            var command = new RefreshTokenCommand(
                dto.AccessToken,
                dto.RefreshToken);

            var tokens = await _mediator.Send(command);

            return Ok(tokens);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _mediator.Send(new LogoutCommand());

            return Ok(new
            {
                Mensaje = "Sesión cerrada correctamente."
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("RecuperacionPassword")]
        [HttpPost("solicitar-recuperacion")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordDto dto)
        {
            await _mediator.Send(
                new ForgotPasswordCommand(dto.Email));

            return Ok(new
            {
                Mensaje = RecuperacionPasswordPolicy.MensajeSolicitud
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("RecuperacionPassword")]
        [HttpPost("validar-otp")]
        public async Task<IActionResult> ValidarOtp(
            [FromBody] ValidarOtpDto dto)
        {
            var resultado = await _mediator.Send(
                new ValidarOtpCommand(
                    dto.Email,
                    dto.Codigo));

            if (!resultado.Exitoso)
            {
                return BadRequest(new
                {
                    Mensaje = resultado.Mensaje
                });
            }

            return Ok(new
            {
                Data = resultado,
                Mensaje = resultado.Mensaje
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("RecuperacionPassword")]
        [HttpPost("restablecer-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            await _mediator.Send(
                new ResetPasswordCommand(
                    dto.Email,
                    dto.ResetToken,
                    dto.NuevaPassword));

            return Ok(new
            {
                Mensaje = "Contraseña restablecida exitosamente."
            });
        }
    }
}