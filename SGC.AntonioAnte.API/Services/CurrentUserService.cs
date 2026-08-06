using SGC.AntonioAnte.Application.Common.Interfaces;
using System.Security.Claims;

namespace SGC.AntonioAnte.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Si el usuario envió un JWT válido, aquí obtenemos su ID (el 'sub' claim)
        public string? UsuarioId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

        public Guid? UsuarioIdGuid => Guid.TryParse(UsuarioId, out var parsedGuid) ? parsedGuid : null;

        // Obtenemos la IP real
        public string IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Desconocida";

        // Obtenemos el Navegador
        public string UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Desconocido";
    }
}
