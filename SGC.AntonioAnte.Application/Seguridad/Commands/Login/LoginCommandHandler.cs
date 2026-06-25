using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.Login
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

            // 1. Validar existencia
            if (usuario == null)
            {
                // SOLUCIÓN: Pasamos _currentUserService.IpAddress y UserAgent en lugar de request
                await RegistrarAuditoria(null, "LOGIN_FALLIDO", "Usuario", null, "Usuario no encontrado", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("Credenciales inválidas.");
            }

            // 2. Validar Estado Activo
            if (!usuario.EstadoActivo)
            {
                await RegistrarAuditoria(usuario.Id, "LOGIN_RECHAZADO", "Usuario", usuario.Id.ToString(), "Cuenta inactiva", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("El usuario se encuentra inactivo en el sistema.");
            }

            // 3. Validar Bloqueos
            if (await _userManager.IsLockedOutAsync(usuario))
            {
                await RegistrarAuditoria(usuario.Id, "CUENTA_BLOQUEADA", "Usuario", usuario.Id.ToString(), "Demasiados intentos fallidos", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("Su cuenta ha sido bloqueada temporalmente por seguridad. Intente de nuevo más tarde.");
            }

            // 4. Validar Contraseña
            if (!await _userManager.CheckPasswordAsync(usuario, request.Password))
            {
                await _userManager.AccessFailedAsync(usuario);
                await RegistrarAuditoria(usuario.Id, "LOGIN_FALLIDO", "Usuario", usuario.Id.ToString(), "Contraseña incorrecta", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
                throw new Exception("Credenciales inválidas.");
            }

            // Login exitoso: reiniciar contador
            await _userManager.ResetAccessFailedCountAsync(usuario);

            // --- Generación de Claims ---
            var roles = await _userManager.GetRolesAsync(usuario);
            var claims = new List<Claim>
            {
                new Claim("sub", usuario.Id.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                new Claim("Departamento", usuario.Departamento)
            };

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            // --- Nueva lógica de Tokens ---
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

            // Generar el Refresh Token
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshTokenValue = $"{refreshTokenString}|{DateTime.UtcNow.AddDays(7):O}";

            // Guardar en la tabla Seguridad.UsuarioTokens
            await _userManager.RemoveAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken");
            await _userManager.SetAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken", refreshTokenValue);

            // 4. Registrar Éxito (CORREGIDO AQUÍ TAMBIÉN)
            await RegistrarAuditoria(usuario.Id, "LOGIN_EXITOSO", "Usuario", usuario.Id.ToString(), "Generación de Access y Refresh Token", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString
            };
        }

        // El método privado se queda exactamente como lo pidió el arquitecto (recibe strings)
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