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
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RefreshTokenCommandHandler(UserManager<Usuario> userManager, IConfiguration configuration, IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Extraer el Principal del Token expirado
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) throw new Exception("Access Token inválido.");

            var userIdString = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId)) throw new Exception("Token no contiene un ID de usuario válido.");

            var usuario = await _userManager.FindByIdAsync(userId.ToString());
            if (usuario == null || !usuario.EstadoActivo) throw new Exception("Usuario no encontrado o inactivo.");

            // 2. Obtener el RefreshToken de la base de datos (Tabla: UsuarioTokens)
            var storedTokenValue = await _userManager.GetAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken");
            if (string.IsNullOrEmpty(storedTokenValue)) throw new Exception("No existe una sesión activa.");

            // Desglosamos nuestro formato: "tokenString|fechaExpiracion"
            var parts = storedTokenValue.Split('|');
            if (parts.Length != 2 || parts[0] != request.RefreshToken)
            {
                await RegistrarAuditoria(usuario.Id, "REFRESH_RECHAZADO", "Token no coincide o está alterado.");
                throw new Exception("Refresh Token inválido.");
            }

            if (DateTime.Parse(parts[1]).ToUniversalTime() <= DateTime.UtcNow)
            {
                await RegistrarAuditoria(usuario.Id, "REFRESH_RECHAZADO", "Refresh Token expirado.");
                throw new Exception("Su sesión ha expirado completamente. Vuelva a iniciar sesión.");
            }

            // 3. Generar nuevos Tokens
            var newAccessToken = GenerarNuevoAccessToken(principal.Claims);
            var newRefreshTokenString = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            var newRefreshTokenValue = $"{newRefreshTokenString}|{DateTime.UtcNow.AddDays(7):O}";

            // 4. Actualizar en la base de datos
            await _userManager.RemoveAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken");
            await _userManager.SetAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken", newRefreshTokenValue);

            await RegistrarAuditoria(usuario.Id, "REFRESH_EXITOSO", "Renovación de sesión exitosa.");

            return new TokenResponseDto { AccessToken = newAccessToken, RefreshToken = newRefreshTokenString };
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // En refresh no es estricto
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!)),
                ValidateLifetime = false // AQUÍ ESTÁ LA CLAVE: Ignoramos que ya haya expirado
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token manipulado o algoritmo incorrecto.");
            }

            return principal;
        }

        private string GenerarNuevoAccessToken(IEnumerable<Claim> claims)
        {
            // Filtramos algunos claims internos que se duplican al regenerar
            var cleanClaims = claims.Where(c => c.Type != "nbf" && c.Type != "exp" && c.Type != "iat").ToList();
            cleanClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: cleanClaims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task RegistrarAuditoria(Guid usuarioId, string accion, string datosAdicionales)
        {
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Entidad = "Usuario",
                EntidadId = usuarioId.ToString(),
                DatosAdicionales = datosAdicionales,
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(default);
        }
    }
}
