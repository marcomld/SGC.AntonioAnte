using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SGC.AntonioAnte.Application.Common.Exceptions;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponseDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public LoginCommandHandler(UserManager<Usuario> userManager, IConfiguration configuration, IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<TokenResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByNameAsync(request.Identificacion);

            if (usuario == null)
            {
                await RegistrarAuditoria(null, "LOGIN_FALLIDO", "Usuario", null, "Usuario no encontrado", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("Credenciales inválidas.");
            }

            if (!usuario.EstadoActivo)
            {
                await RegistrarAuditoria(usuario.Id, "LOGIN_RECHAZADO", "Usuario", usuario.Id.ToString(), "Cuenta inactiva", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("El usuario se encuentra inactivo en el sistema.");
            }

            // 1. CONTROL DE LOCKOUT EXISTENTE PRE-LOGIN
            if (await _userManager.IsLockedOutAsync(usuario))
            {
                var fechaFinBloqueo = usuario.LockoutEnd ?? DateTimeOffset.UtcNow;
                var segundosRestantes = Math.Max(0, (int)(fechaFinBloqueo - DateTimeOffset.UtcNow).TotalSeconds);

                await RegistrarAuditoria(usuario.Id, "CUENTA_BLOQUEADA_CONSULTA", "Usuario", usuario.Id.ToString(), $"Intento de acceso en cuenta penalizada. Segundos restantes: {segundosRestantes}", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);

                throw new LoginBloqueadoException(segundosRestantes);
            }

            // 2. VALIDAR CONTRASEÑA CON LOCKOUT ACTIVADO
            if (!await _userManager.CheckPasswordAsync(usuario, request.Password))
            {
                await _userManager.AccessFailedAsync(usuario);

                // Verificar si este fallo causó un nuevo bloqueo inmediato
                if (await _userManager.IsLockedOutAsync(usuario))
                {
                    var fechaFinBloqueo = usuario.LockoutEnd ?? DateTimeOffset.UtcNow.AddMinutes(15);
                    var segundosRestantes = Math.Max(0, (int)(fechaFinBloqueo - DateTimeOffset.UtcNow).TotalSeconds);

                    await RegistrarAuditoria(usuario.Id, "CUENTA_BLOQUEADA", "Usuario", usuario.Id.ToString(), "Límite de intentos alcanzado. Cuenta penalizada.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);

                    throw new LoginBloqueadoException(segundosRestantes);
                }

                await RegistrarAuditoria(usuario.Id, "LOGIN_FALLIDO", "Usuario", usuario.Id.ToString(), "Contraseña incorrecta", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("Credenciales inválidas.");
            }

            await _userManager.ResetAccessFailedCountAsync(usuario);

            // --- Generación de Claims y Tokens ---
            var roles = await _userManager.GetRolesAsync(usuario);
            var claims = new List<Claim>
            {
                new Claim("sub", usuario.Id.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                new Claim("DepartamentoId", usuario.DepartamentoId?.ToString() ?? string.Empty)
            };

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);
            var refreshTokenValue = $"{refreshTokenString}|{DateTime.UtcNow.AddDays(7):O}";

            await _userManager.RemoveAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken");
            await _userManager.SetAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken", refreshTokenValue);

            await RegistrarAuditoria(usuario.Id, "LOGIN_EXITOSO", "Usuario", usuario.Id.ToString(), "Generación de Access y Refresh Token", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString
            };
        }

        private async Task RegistrarAuditoria(Guid? usuarioId, string accion, string entidad, string? entidadId, string datosAdicionales, string ipAddress, string userAgent, CancellationToken cancellationToken)
        {
            var auditoria = new Auditoria
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                DatosAdicionales = datosAdicionales,
                DireccionIp = ipAddress,
                Navegador = userAgent,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Auditorias.Add(auditoria);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}